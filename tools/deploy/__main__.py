# ========================================================================================================
# Reality Deploy Tool
# 
# A script used to deploy Reality to Docker Compose or Kubernetes.
# Note: this is a work in progress and is not yet complete.
#
# Usage:
#   python -m tools.deploy [command]
#
# Commands:
#   build       Builds the Docker images for Reality.
#   up          Starts Reality.
#   down        Stops Reality.
#
# Options:
#   -m, --mode  The mode to build the images in. Can be 'compose' or 'kubernetes'.
#   -p, --push  Pushes the images to the registry after building.
#
# ========================================================================================================

from configuration import Configuration
from dialogs import Dialogs
import os
from rich.prompt import Prompt
import subprocess
import typer
from typing import Tuple
from typing_extensions import Annotated

# Directories
tool_directory = os.path.dirname(os.path.realpath(__file__))
root_directory = os.path.join(tool_directory, '..', '..')
src_directory = os.path.join(root_directory, 'src')

# Definitions
app = typer.Typer()
dialogs = Dialogs()

# Configurations
reality_services_config = Configuration()
reality_services_config.load_from_file(os.path.join(root_directory, 'services.yml'))

kubectl = ["kubectl"]
registry_host = None # Set to None to push to docker.io.
secrets_filename = 'secrets.yml'

# Argument and option validation functions
def validate_mode_option(mode: str):
    mode = mode.lower()

    compose = ["compose", "docker"]
    kubernetes = ["kubernetes", "k8s"]

    if (mode not in compose and mode not in kubernetes):
        raise typer.BadParameter("Invalid mode '" + mode + "'.")
    
    if (mode in compose):
        return "Compose"
    else:
        return "Kubernetes"

# Task functions
def check_docker(display: bool = False) -> Tuple[bool, list]:
    try:
        check_step = subprocess.Popen(["docker", "version"], cwd=root_directory, stderr=subprocess.PIPE)
        check_step.wait()
        output, _ = check_step.communicate()
        if (check_step.returncode == 0):
            if (display):
                dialogs.console.print("Found " + output, style="gray")
            return True, ["docker"]
        else:
            return False, []
    except Exception:
        pass

    return False, []

def check_docker_compose(display: bool = False) -> Tuple[bool, list]:
    try: 
        check_step = subprocess.Popen(["docker-compose", "version"], cwd=root_directory, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        check_step.wait()
        output, _ = check_step.communicate()
        if (check_step.returncode == 0):
            if (display):
                dialogs.console.print("Found " + output, style="gray")
            return True, ["docker-compose"]
    except Exception:
        pass
    
    try:
        check_step = subprocess.Popen("docker", "compose", "version", cwd=root_directory, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        check_step.wait()
        output, _ = check_step.communicate()
        if (check_step.returncode == 0):
            if (display):
                dialogs.console.print("Found " + output, style="gray")
            return True, ["docker", "compose"]
    except Exception:
        pass

    return False, []

def check_kubernetes(display: bool = False) -> Tuple[bool, list]:
    try:
        check_step = subprocess.Popen(kubectl + ["version"], cwd=root_directory, stderr=subprocess.PIPE)
        check_step.wait()
        output, _ = check_step.communicate()
        if (check_step.returncode == 0):
            if (display):
                dialogs.console.print("Found " + output, style="gray")
            return True, kubectl
        else:
            return False, []
    except Exception:
        pass

    return False, []

def login_docker_registry() -> str:
    # Login to registry
    username = Prompt.ask("Enter your registry username: ", default="")
    password = Prompt.ask("Enter your registry password: ", default="", password=True)

    login_step = subprocess.Popen(["docker", "login", registry_host or "", "-u", username, "-p", password], stderr=subprocess.PIPE)
    login_step.wait()
    _, error = login_step.communicate()
    if (login_step.returncode != 0):
        dialogs.error("Failed to login to registry.")
        dialogs.console.print_exception(error, style="red")
        exit(1)

    return username

def build_compose(push: bool):
    # Check if docker-compose is installed.
    _, cmd = check_docker_compose()

    dialogs.warn("Warning! Compose mode will push images to the registry according to each services 'image' property.")
    dialogs.warn("If you want to push to a registry, please edit the 'docker-compose.yml' file and set 'image' property for each service.")

    # Build images
    # TODO: Properly read the component definition from services.yml to determine if it should be built.
    dialogs.info("Building images...")
    build_step = subprocess.Popen(cmd + ["build"], cwd=root_directory, stdout= subprocess.STDOUT, stderr=subprocess.PIPE)
    build_step.wait()
    _, error = build_step.communicate()
    if (build_step.returncode != 0):
        dialogs.error("Failed to build images.")
        dialogs.console.print(error, style="red")
        exit(1)

    if (push): # TODO: Properly read the component definition from services.yml to determine if it should be pushed.
        dialogs.info("[bold blue]Pushing to registry...")
        push_step = subprocess.Popen(cmd + ["push"], cwd=root_directory, stdout= subprocess.STDOUT, stderr=subprocess.PIPE)
        push_step.wait()
        _, error = push_step.communicate()
        if (push_step.returncode != 0):
            dialogs.error("Failed to push images.")
            dialogs.console.print(error, style="red")
            exit(1)

    dialogs.done("Done.")

def build_kubernetes(push: bool, username: str):
    dialogs.warn("Warning! Kubernetes mode builds and pushes images to the registry according to the 'services.yml' file.")
    dialogs.warn("If you want to push to a different registry, please edit the 'services.yml' file and set 'registry_host' property.")

    created_images = []

    # TODO: Properly read the component definition from services.yml to determine if it should be built.
    for component in reality_services_config.components:
        dialogs.debug("Building " + component.id + " from folder " + component.path + "...")

        service_folder = os.path.join(root_directory, component.path)
        dockerfile = os.path.join(service_folder, "Dockerfile")
        tag = registry_host and os.path.join(registry_host, component.id) or os.path.join(username, component.id)

        has_dockerfile = os.path.isfile(dockerfile)
        if (not has_dockerfile):
            dialogs.debug("No Dockerfile found in " + service_folder + ". Skipping...")
            continue

        build_step = subprocess.Popen(["docker", "build", ".", "-f", os.path.join(component.path, "Dockerfile"), "-t", tag],
                                      cwd=root_directory, stdout= subprocess.STDOUT, stderr=subprocess.PIPE)
        build_step.wait()
        _, error = build_step.communicate()
        if (build_step.returncode != 0):
            dialogs.error("Failed to build " + component.id + ".")
            dialogs.console.print_exception(error, style="red")
            exit(1)

        if (push): # TODO: Properly read the component definition from services.yml to determine if it should be pushed.
            dialogs.info("Pushing " + component.id + " to registry...")
            push_step = subprocess.Popen(["docker", "push", tag], stdout= subprocess.STDOUT, stderr=subprocess.PIPE)
            push_step.wait()
            _, error = push_step.communicate()
            if (build_step.returncode != 0):
                dialogs.error("Failed to push " + component.id + ".")
                dialogs.console.print_exception(error, style="red")
                exit(1)

        created_images.append(tag)

    dialogs.done("Built images:")
    for image in created_images:
        dialogs.console.print("* " + image)
    dialogs.done("[bold green]\nDone.\n")

def start_compose(build: bool, restart: bool):
    # Check if docker-compose is installed.
    installed, cmd = check_docker_compose(display=True)
    if (not installed):
        dialogs.alert_docker_compose_not_found()
        exit(1)

    if (restart):
        stop_compose()

    if (build):
        build_compose(False)

    start_step = subprocess.Popen(cmd + ["up", "-d"], cwd=root_directory, stdout=subprocess.STDOUT, stderr=subprocess.PIPE)
    start_step.wait()
    _, error = start_step.communicate()
    if (start_step.returncode != 0):
        dialogs.error("Failed to start Reality on Compose mode.")
        dialogs.console.print_exception(error, style="red")
        exit(1)

def start_kubernetes(build: bool, restart: bool, ignore: bool):
    # Check if docker-compose is installed.
    installed, _ = check_kubernetes(display=True)
    if (not installed):
        dialogs.alert_kubectl_not_found()
        exit(1)

    if (restart):
        stop_kubernetes()

    if (build):
        username = login_docker_registry()
        build_kubernetes(False, username)

    # Apply secrets
    dialogs.info("Applying secrets...")
    secret_step = subprocess.Popen(kubectl + ["apply", "-f", secrets_filename],
                                   cwd=root_directory, stdout= subprocess.STDOUT, stderr=subprocess.PIPE)
    secret_step.wait()
    _, error = secret_step.communicate()
    if (secret_step.returncode != 0):
        dialogs.error("Failed to apply secrets.")
        dialogs.console.print_exception(error, style="red")
        exit(1)

    # Apply service configurations
    dialogs.info("Applying service configurations...")
    for component in reality_services_config.components:
        print("* Applying " + component.id + "...")
        service_file = os.path.join(component.path, "kubernetes.yml")
        apply_step = subprocess.Popen(kubectl + ["apply", "-f", service_file],
                                      cwd=src_directory, stdout= subprocess.STDOUT, stderr=subprocess.PIPE)
        apply_step.wait()
        _, error = apply_step.communicate()
        if (apply_step.returncode != 0):
            dialogs.error("Failed to apply service " + component.id + ".")
            dialogs.console.print_exception(error, style="red")
            if (ignore):
                dialogs.console.print("[italic gray]Continuing...")
            else:
                exit(1)

def stop_compose():
    # Check if docker-compose is installed.
    installed, cmd = check_docker_compose()
    if (not installed):
        dialogs.alert_docker_compose_not_found()
        exit(1)

    dialogs.warn("Stopping Reality on Compose mode...")

    stop_step = subprocess.Popen(cmd + ["down"], cwd=root_directory,
                                 stdout= subprocess.STDOUT, stderr=subprocess.PIPE)
    stop_step.wait()
    _, error = stop_step.communicate()
    if (stop_step.returncode != 0):
        print(error)
        exit(1)

def stop_kubernetes():
    # Check if kubectl is installed.
    installed = check_kubernetes()
    if (not installed):
        dialogs.alert_kubectl_not_found()
        exit(1)

    dialogs.warn("Stopping Reality on Kubernetes mode...")

    stop_step = subprocess.Popen(kubectl + ["delete", "pods,deployments,services", "--all"],
                                 cwd=root_directory, stdout= subprocess.STDOUT, stderr=subprocess.PIPE)
    stop_step.wait()
    _, error = stop_step.communicate()
    if (stop_step.returncode != 0):
        print(error)
        exit(1)


# Commands

@app.command("build")
def build(
    mode: Annotated[str, typer.Option("-m", help="The mode to build the images in.", callback=validate_mode_option)] = "Compose",
    push: Annotated[bool, typer.Option("-p", help="Pushes the images to the registry after building.")] = True
):
    dialogs.info("Building images on " + mode + " mode...")

    # Check if Docker is installed.
    installed, _ = check_docker()
    if (not installed):
        dialogs.alert_docker_not_found()
        exit(1)

    if (mode == "Compose"):
        # Check if docker-compose is installed.
        installed, _ = check_docker_compose()
        if (not installed):
            dialogs.alert_docker_compose_not_found()
            exit(1)
    elif (mode == "Kubernetes"):
        # Check if kubectl is installed.
        installed, _ = check_kubernetes()
        if (not installed):
            dialogs.alert_kubectl_not_found()
            exit(1)

    # Login to registry
    username = login_docker_registry()

    if (mode == "Compose"):
        build_compose(push)
    elif (mode == "Kubernetes"):
        build_kubernetes(push, username)


@app.command("up")
def start(
    mode: Annotated[str, typer.Option("-m", help="The mode to build the images in.", callback=validate_mode_option)] = "Compose",
    ignore: Annotated[str, typer.Option(help="Ignores if a configuration file fails and continues deploying.")] = False,
    build: Annotated[bool, typer.Option("-b", help="Builds the images before starting.")] = False,
    restart: Annotated[bool, typer.Option("-r", help="Kills the services before starting.")] = False,
):
    dialogs.info("Starting Reality on " + mode + " mode!")
    
    if (mode == "Compose"):
        start_compose(build, restart)
    elif (mode == "Kubernetes"):
        start_kubernetes(build, restart, ignore)


@app.command("down")
def down(
    mode: Annotated[str, typer.Option("-m", help="The mode to build the images in.", callback=validate_mode_option)] = "Compose",
):
    if (mode == "Compose"):
        stop_compose()
    elif (mode == "Kubernetes"):
        stop_kubernetes()


if __name__ == "__main__":
    dialogs.welcome()
    app()