import os
from typing_extensions import Annotated
import typer
import subprocess

# Configurations
registry_host = 'localhost:5000'
secrets_filename = 'secrets.yml'
service_order = [
    'Reality.Database',
    'Reality.Services.Identity',
    'Reality.Services.UPx',
    'Reality.Services.Portfolio',
    'Portfolio',
    'Reality.Gateway',
    'Reality.Proxy',
]

main_directory = os.path.pardir(os.pardir(os.getcwd()))
src_directory = os.path.join(main_directory, 'src')

# App
app = typer.Typer()

@app.command("build")
def build_images():
    print("Building Docker images...")

    login_step = subprocess.Popen(["docker", "login", registry_host or ""], stderr=subprocess.PIPE)
    login_step.wait()
    error = login_step.communicate()[1]
    if (login_step.returncode != 0):
        print(error)
        exit(1)

    created_images = []

    # Build images
    for folder in service_order:
        print("Building " + folder + "...")
        name = folder.lower()

        service_folder = os.path.join(src_directory, folder)
        dockerfile = os.path.join(service_folder, "Dockerfile")
        tag = os.path.join(registry_host, name)

        has_dockerfile = os.path.isfile(dockerfile)
        if (not has_dockerfile):
            print("No Dockerfile found in " + folder + ". Skipping...")
            continue

        build_step = subprocess.Popen(["docker", "build", ".", "-f", os.path.join(folder, "Dockerfile"), "-t", tag],
                                      cwd=src_directory, stderr=subprocess.PIPE)
        build_step.wait()
        error = build_step.communicate()[1]
        if (build_step.returncode != 0):
            print(error)
            exit(1)

        print("Pushing " + folder + " to registry...")
        push_step = subprocess.Popen(["docker", "push", tag], stderr=subprocess.PIPE)
        push_step.wait()
        error = push_step.communicate()[1]
        if (build_step.returncode != 0):
            print(error)
            exit(1)

        created_images.append(tag)

    print("Built images:")
    for image in created_images:
        print("- " + image)
    print("Done.")

@app.command("up")
def start_kubernetes(
    ignore: Annotated[str, typer.Option(help="Ignores if a configuration file fails and continues deploying.")] = False
):
    print("Starting Reality...")
    
    # Apply secrets
    secret_step = subprocess.Popen(["kubectl", "apply", "-f", secrets_filename], cwd=main_directory, stderr=subprocess.PIPE)
    secret_step.wait()
    error = secret_step.communicate()[1]
    if (secret_step.returncode != 0):
        print(error)
        exit(1)

    # Apply service configurations
    for folder in service_order:
        service_file = os.path.join(folder, "kubernetes.yml")
        apply_step = subprocess.Popen(["kubectl", "apply", "-f", service_file], cwd=src_directory, stderr=subprocess.PIPE)
        apply_step.wait()
        error = apply_step.communicate()[1]
        if (apply_step.returncode != 0):
            print(error)
            if (ignore):
                print("* Continuing...")
            else:
                exit(1)

@app.command("update")
def update_kubernetes():
    print("Updating Kubernetes configuration...")

    # Apply service configurations
    for folder in service_order:
        service_file = os.path.join(folder, "kubernetes.yml")
        apply_step = subprocess.Popen(["kubectl", "apply", "-f", service_file], cwd=src_directory, stderr=subprocess.PIPE)
        apply_step.wait()
        error = apply_step.communicate()[1]
        if (apply_step.returncode != 0):
            print(error)
            exit(1)

@app.command("down")
def stop_kubernetes():
    print("Stopping Reality...")

    stop_step = subprocess.Popen(["kubectl", "delete", "all", "--all"], cwd=main_directory, stderr=subprocess.PIPE)
    stop_step.wait()
    error = stop_step.communicate()[1]
    if (stop_step.returncode != 0):
        print(error)
        exit(1)
    
app()