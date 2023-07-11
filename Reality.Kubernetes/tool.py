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
        dockerfile = os.path.join('./', folder, 'Dockerfile')
        tag = os.path.join(registry_host, name)
        subprocess.run(["docker", "build", "-t", tag, "-f", dockerfile, "."])

        print("Pushing " + folder + " to local registry...")
        subprocess.run(["docker", "push", tag])

@app.command("up")
def start_kubernetes():
    print("Starting Reality...")
    
    # Apply secrets
    secrets = os.path.join('../', secrets_filename)
    subprocess.run(["kubectl", "apply", "-f", secrets])

    # Apply service configurations
    for folder in service_folder_order:
        service = os.path.join('./', folder, "kubernetes.yml")
        subprocess.run(["kubectl", "apply", "-f", service])

@app.command("update")
def update_kubernetes():
    print("Updating Kubernetes configuration...")

    # Apply service configurations
    for folder in service_folder_order:
        service = os.path.join('./', folder, "kubernetes.yml")
        subprocess.run(["kubectl", "apply", "-f", service])

@app.command("down")
def stop_kubernetes():
    print("Stopping Reality...")

    subprocess.run(["kubectl", "delete", "-all"])
    
app()