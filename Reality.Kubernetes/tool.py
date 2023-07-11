import os
import typer
import subprocess

# Configuration files
registry_host = 'localhost:5000'
secrets_filename = 'secrets.yml'
service_folder_order = [
    'Reality.Database',
    'Reality.Services.Identity',
    'Reality.Services.UPx',
    'Reality.Services.Portfolio',
    'Portfolio',
    'Reality.Gateway',
    'Reality.Proxy',
]


# App
app = typer.Typer()

@app.command("build")
def build_images():
    print("Building Docker images...")
    subprocess.run(["docker", "login", registry_host])

    # Build images
    for folder in service_folder_order:
        print("Building " + folder + "...")
        name = folder.lower()
        dockerfile = os.path.join('../', folder, 'Dockerfile')
        tag = os.path.join(registry_host, name)
        build_step = subprocess.Popen(["docker", "build", "-t", tag, "-f", dockerfile, "."])
        build_step.wait()

        print("Pushing " + folder + " to local registry...")
        push_step = subprocess.Popen(["docker", "push", tag])
        push_step.wait()

@app.command("up")
def start_kubernetes():
    print("Starting Reality...")
    
    # Apply secrets
    secrets = os.path.join('../', secrets_filename)
    subprocess.run(["kubectl", "apply", "-f", secrets])

    # Apply service configurations
    for folder in service_folder_order:
        service = os.path.join('../', folder, "kubernetes.yml")
        apply_step = subprocess.Popen(["kubectl", "apply", "-f", service])
        apply_step.wait()

@app.command("update")
def update_kubernetes():
    print("Updating Kubernetes configuration...")

    # Apply service configurations
    for folder in service_folder_order:
        service = os.path.join('../', folder, "kubernetes.yml")
        apply_step = subprocess.Popen(["kubectl", "apply", "-f", service])
        apply_step.wait()

@app.command("down")
def stop_kubernetes():
    print("Stopping Reality...")

    stop_step = subprocess.Popen(["kubectl", "delete", "-all"])
    stop_step.wait()
    
app()