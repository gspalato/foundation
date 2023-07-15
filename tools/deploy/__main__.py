# ========================================================================================================
# Reality Deployment Tool
# 
# A script used to deploy Reality to Docker Compose or Kubernetes.
# Note: this is a work in progress and is not yet complete.
#
# Usage:
#   python3 tools/deploy [command]
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
from os import path
from rich.prompt import Prompt
import subprocess
import typer
from typing import Tuple
from typing_extensions import Annotated

# Directories
file_directory = path.dirname(path.realpath(__file__))
tool_directory = path.dirname(file_directory)
root_directory = path.dirname(tool_directory)
src_directory = path.join(root_directory, 'src')

# Definitions
app = typer.Typer()
dialogs = Dialogs()

# Configurations
reality_services_config = Configuration()
reality_services_config.load_from_file(path.join(root_directory, 'services.yml'))

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

# Helper functions
def execute_command(cmd: list[str], cwd: str = None, *args) -> Tuple[int, str, str]:
    step = subprocess.Popen(cmd, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    step.wait()
    output, error = step.communicate(args)
    return step.returncode, output, error

# Task functions
def check_docker(display: bool = False) -> Tuple[bool, list]:
    import shutil

    path = shutil.which("docker")
    if (path):
        if (display):
            dialogs.console.log("Found Docker.", style="bright_black")
        return True, ["docker"]
    else:
        return False, []

def check_docker_compose() -> Tuple[bool, list]:
    import shutil

    # First check for an older docker-compose command.
    path = shutil.which("docker-compose")
    if (path):
        if (display):
            dialogs.console.log("Found Docker Compose.", style="bright_black")
        return True, ["docker-compose"]
    
    docker_exists, _ = check_docker()
    if (not docker_exists):
        return False, []

    # Then, check for the newer docker compose command.
    try:
        code, _, _ = execute_command(["docker", "compose", "version"], cwd=root_directory)
        if (code == 0):
            dialogs.console.log("Found Docker Compose.", style="bright_black")
            return True, ["docker", "compose"]
    except Exception:
        pass

    return False, []

def check_kubernetes(display: bool = False) -> Tuple[bool, list]:
    try:
        code, output, _ = execute_command(kubectl + ["version"], cwd=root_directory)
        if (code == 0):
            if (display):
                dialogs.console.log("Found " + output, style="bright_black")
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

    code, _, error = execute_command(["docker", "login", registry_host or "", "-u", username, "-p", password])
    if (code != 0):
        dialogs.error("Failed to login to registry.")
        dialogs.console.print_exception(error, style="red")
        exit(1)

    return username

def build_compose(push: bool):
    # Check if docker-compose is installed.
    _, cmd = check_docker_compose()

    dialogs.warn("Warning! Compose mode will push images to the registry according to each services 'image' property.")
    dialogs.warn("If you want to push to a registry, please edit the 'docker-compose.yml' file and set 'image' property for each service.")

    with dialogs.console.status("[bold blue]Building container images...") as status:
        # Build images
        # TODO: Properly read the component definition from services.yml to determine if it should be built.
        code, _, error = execute_command(cmd + ["build"], cwd=root_directory)
        if (code != 0):
            dialogs.error("Failed to build images.")
            dialogs.console.log(error, style="red")
            exit(1)

        if (push): # TODO: Properly read the component definition from services.yml to determine if it should be pushed.
            dialogs.info("[bold blue]Pushing to registry...")
            code, _, error = execute_command(cmd + ["push"], cwd=root_directory)
            if (code != 0):
                dialogs.error("Failed to push images.")
                dialogs.console.print_exception(error, style="red")
                exit(1)

    dialogs.done("Built container images on Compose mode.")

def build_kubernetes(push: bool, username: str):
    dialogs.warn("Warning! Kubernetes mode builds and pushes images to the registry according to the 'services.yml' file.")
    dialogs.warn("If you want to push to a different registry, please edit the 'services.yml' file and set 'registry_host' property.")

    created_images = []

    with dialogs.console.status("[bold blue]Building images...") as status:

        # TODO: Properly read the component definition from services.yml to determine if it should be built.
        for component in reality_services_config.components:
            status.update("Building " + component.id + " from folder " + component.path + "...")

            service_folder = path.join(root_directory, component.path)
            dockerfile = path.join(service_folder, "Dockerfile")
            tag = registry_host and path.join(registry_host, component.id) or path.join(username, component.id)

            has_dockerfile = path.isfile(dockerfile)
            if (not has_dockerfile):
                dialogs.debug("No Dockerfile found in " + service_folder + ". Skipping...")
                continue

            code, _, error = execute_command(["docker", "build", ".", "-f", path.join(component.path, "Dockerfile"), "-t", tag],
                                             cwd=root_directory)
            if (code != 0):
                dialogs.error("Failed to build " + component.id + ".")
                dialogs.console.print_exception(error, style="red")
                exit(1)

            if (push): # TODO: Properly read the component definition from services.yml to determine if it should be pushed.
                status.update("Pushing " + component.id + " to registry...")
                code, _, error = execute_command(["docker", "push", tag], cwd=root_directory)
                if (code != 0):
                    dialogs.error("Failed to push " + component.id + ".")
                    dialogs.console.print_exception(error, style="red")
                    exit(1)

            created_images.append(tag)

    dialogs.console.log("Built container images on Kubernetes mode:")
    for image in created_images:
        dialogs.console.print("* " + image, style="bright_black")

    dialogs.done("\nDone.\n")

def start_compose(build: bool, restart: bool):
    # Check if docker-compose is installed.
    installed, cmd = check_docker_compose()
    if (not installed):
        dialogs.alert_docker_compose_not_found()
        exit(1)

    if (restart):
        stop_compose()

    if (build):
        build_compose(False)

    with dialogs.console.status("[bold blue]Starting Reality on Compose mode..."):
        code, _, error = execute_command(cmd + ["up", "-d"], cwd=root_directory)
        if (code != 0):
            dialogs.error("Failed to start Reality on Compose mode.")
            dialogs.console.print_exception(error, style="red")
            exit(1)
    
    dialogs.done("Started Reality on Compose mode.")

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

    with dialogs.console.status("[bold blue]Starting Reality on Kubernetes mode...") as status:
        # Apply secrets
        status.update("[bold blue]Applying secrets...")
        code, _, error = execute_command(kubectl + ["apply", "-f", secrets_filename],
                                         cwd=src_directory)
        if (code != 0):
            dialogs.error("Failed to apply secrets.")
            dialogs.console.print_exception(error, style="red")
            exit(1)

        # Apply service configurations
        status.update("Applying service configurations...")
        for component in reality_services_config.components:
            dialogs.console.log("* Applying " + component.id + "...")
            service_file = path.join(component.path, "kubernetes.yml")

            code, _, error = execute_command(kubectl + ["apply", "-f", service_file],
                                             cwd=src_directory)
            if (code != 0):
                dialogs.error("[bold red]Failed to apply service " + component.id + ".")
                dialogs.console.print_exception(error, style="red")
                if (ignore):
                    dialogs.console.log("[italic bright_black]Continuing...")
                else:
                    exit(1)
    
    dialogs.done("Started Reality on Kubernetes mode.")

def stop_compose():
    # Check if docker-compose is installed.
    installed, cmd = check_docker_compose()
    if (not installed):
        dialogs.alert_docker_compose_not_found()
        exit(1)

    with dialogs.console.status("Stopping Reality on Compose mode..."):
        code, _, error = execute_command(cmd + ["down"], cwd=root_directory)
        if (code != 0):
            dialogs.console.print_exception(error)
            exit(1)

    dialogs.done("Done.")

def stop_kubernetes():
    # Check if kubectl is installed.
    installed = check_kubernetes()
    if (not installed):
        dialogs.alert_kubectl_not_found()
        exit(1)

    with dialogs.console.status("Stopping Reality on Kubernetes mode..."):
        code, _, error = execute_command(kubectl + ["delete", "pods,deployments,services", "--all"],
                                         cwd=root_directory)
        if (code != 0):
            dialogs.console.print_exception(error)
            exit(1)

    dialogs.done("Done.")


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