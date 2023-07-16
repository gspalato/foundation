from os import path
from rich.panel import Panel
import typer
from typer import Typer
from typing_extensions import Annotated

from lib.configuration import configuration
from lib.console import console
from lib.directories import ROOT_DIR, SRC_DIR
from lib.checks import Checks
from lib.shell import Shell
from lib.utils import Utils

# Create the app.
app: Typer = Typer()

# Add the subcommands.
@app.command("build", help="Builds the container images.")
def build_command(
    push: Annotated[bool, typer.Option("-p", help="Pushes the images to the registry after building.")] = True
) -> str:
    console.warn("Warning! Kubernetes mode builds and pushes images to the registry according to the 'services.yml' file.")
    console.warn("If you want to push to a different registry, please edit the 'services.yml' file and set 'registry' property.")

    registry = configuration.settings["registry"]
    created_images = []

    username = Utils.login_to_registry()

    with console.status("[bold blue]Building images...") as status:

        # TODO: Properly read the component definition from reality.yml to determine if it should be built.
        for component in configuration.components:
            status.update("Building " + component.id + " from folder " + component.path + "...")

            service_folder = path.join(ROOT_DIR, component.path)
            dockerfile = path.join(service_folder, "Dockerfile")
            tag = registry and path.join(registry, component.id) or path.join(username, component.id)

            has_dockerfile = path.isfile(dockerfile)
            if (not has_dockerfile):
                console.debug("No Dockerfile found in " + service_folder + ". Skipping...")
                continue

            code, _, error = Shell.execute(["docker", "build", ".", "-f", component.build.dockerfile, "-t", tag],
                                             cwd=path.join(ROOT_DIR, component.build.context))
            if (code != 0):
                console.error("Failed to build " + component.id + ".")
                console.error_panel(error)
                exit(1)

            if (push): # TODO: Properly read the component definition from reality.yml to determine if it should be pushed.
                status.update("Pushing " + component.id + " to registry...")
                code, _, error = Shell.execute(["docker", "push", tag], cwd=ROOT_DIR)
                if (code != 0):
                    console.error("Failed to push " + component.id + ".")
                    console.error_panel(error)
                    exit(1)

            created_images.append(tag)

    image_list = "\n".join([("* " + x) for x in created_images])
    panel = Panel.fit(image_list, title="Images", border_style="blue")

    console.log("Built container images on Kubernetes mode!")
    console.print(panel)

    console.done("\nDone.\n")

    return username

@app.command("up", help="Starts Reality on Kubernetes mode. Creates services, deployments and pods for each Reality service.")
def up_command(
    ignore: Annotated[str, typer.Option(help="Ignores if a configuration file fails and continues deploying.")] = False,
    build: Annotated[bool, typer.Option("-b", help="Builds the images before starting.")] = False,
    restart: Annotated[bool, typer.Option("-r", help="Kills the services before starting.")] = False,
):
    # Check if docker-compose is installed.
    installed, cmd = Checks.get_kubernetes()
    if (not installed):
        console.alert_kubectl_not_found()
        exit(1)

    if (restart):
        down_command()

    if (build):
        username = build_command(False, username)

    with console.console.status("[bold blue]Starting Reality on Kubernetes mode...") as status:
        # Apply secrets
        status.update("[bold blue]Applying secrets...")
        code, _, error = Shell.execute(cmd + ["apply", "-f", configuration.settings["secrets_file"]],
                                         cwd=SRC_DIR)
        if (code != 0):
            console.error("Failed to apply secrets.")
            console.error_panel(error)
            exit(1)

        # Apply service configurations
        status.update("Applying service configurations...")
        for component in configuration.components:
            console.log("* Applying " + component.id + "...")
            service_file = path.join(component.path, "kubernetes.yml")

            code, _, error = Shell.execute(cmd + ["apply", "-f", service_file],
                                             cwd=SRC_DIR)
            if (code != 0):
                console.error("[bold red]Failed to apply service " + component.id + ".")
                console.error_panel(error)
                if (ignore):
                    console.log("[italic bright_black]Continuing...")
                else:
                    exit(1)
    
    console.done("Started Reality on Kubernetes mode.")

@app.command("down", help="Stops Reality on Kubernetes mode and deletes all services, deployments and pods.")
def down_command():
    # Check if kubectl is installed.
    installed, cmd = Checks.get_kubernetes()
    if (not installed):
        console.alert_kubectl_not_found()
        exit(1)

    with console.status("Stopping Reality on Kubernetes mode..."):
        code, _, error = Shell.execute(cmd + ["delete", "pods,deployments,services", "--all"],
                                         cwd=ROOT_DIR)
        if (code != 0):
            console.error_panel(error)
            exit(1)

    console.done("Done.")

@app.command("restart")
def restart_command():
    """To be implemented."""
    pass

@app.command("status")
def status_command():
    """To be implemented."""
    pass

@app.command("logs")
def logs_command():
    """To be implemented."""
    pass