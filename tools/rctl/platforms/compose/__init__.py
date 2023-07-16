from rich.columns import Columns
from rich.table import Table
import typer
from typer import Typer
from typing_extensions import Annotated

from lib.configuration import configuration
from lib.console import console
from lib.directories import ROOT_DIR
from lib.checks import Checks
from lib.shell import Shell

from platforms.compose.api import client

# Create the app.
app: Typer = Typer()

# Add the subcommands.
@app.command("build")
def build_command(
    push: Annotated[bool, typer.Option("-p", help="Pushes the images to the registry after building.")] = True
):
    """Builds the container images."""

    # Check if Docker Compose is installed.
    installed, cmd = Checks.get_docker_compose()
    if (not installed):
        console.alert_docker_compose_not_found()
        exit(1)

    console.warn("Warning! Compose mode will push images to the registry according to each services 'image' property.")
    console.warn("If you want to push to a registry, please edit the 'docker-compose.yml' file and set 'image' property for each service.")

    with console.status("[bold blue]Building container images...") as status:
        # Build images
        # TODO: Properly read the component definition from services.yml to determine if it should be built.
        code, _, error = Shell.execute(cmd + ["build"], cwd=ROOT_DIR)
        if (code != 0):
            console.error("Failed to build images.")
            console.log(error, style="red")
            exit(1)

        if (push): # TODO: Properly read the component definition from services.yml to determine if it should be pushed.
            status.update("Pushing to registry...")
            code, _, error = Shell.execute(cmd + ["push"], cwd=ROOT_DIR)
            if (code != 0):
                console.error("Failed to push images.")
                console.error_panel(error)
                exit(1)

    console.done("Built container images on Compose mode.")

@app.command("up")
def up_command(
    build: Annotated[bool, typer.Option("-b", help="Builds the images before starting.")] = False,
    restart: Annotated[bool, typer.Option("-r", help="Kills the services before starting.")] = False,
):
    """Starts Reality on Compose mode."""

    # Check if Docker Compose is installed.
    installed, cmd = Checks.get_docker_compose()
    if (not installed):
        console.alert_docker_compose_not_found()
        exit(1)

    if (restart):
        down_command()

    if (build):
        build_command(False)

    with console.status("[bold blue]Starting Reality on Compose mode..."):
        code, _, error = Shell.execute(cmd + ["up", "-d"], cwd=ROOT_DIR)
        if (code != 0):
            console.error("Failed to start Reality on Compose mode.")
            console.console.error_panel(error)
            exit(1)
    
    console.done("Started Reality on Compose mode.")

@app.command("down")
def down_command():
    """Stops Reality on Compose mode and deletes all containers."""

    # Check if Docker Compose is installed.
    installed, cmd = Checks.get_docker_compose()
    if (not installed):
        console.alert_docker_compose_not_found()
        exit(1)

    with console.status("Stopping Reality on Compose mode..."):
        code, _, error = Shell.execute(cmd + ["down"], cwd=ROOT_DIR)
        if (code != 0):
            console.error_panel(error)
            exit(1)

    console.done("Stopped Reality on Compose.")

@app.command("restart")
def restart_command():
    """Restarts Reality on Compose mode."""

    # Check if Docker Compose is installed.
    installed, cmd = Checks.get_docker_compose()
    if (not installed):
        console.alert_docker_compose_not_found()
        exit(1)

    with console.status("Restarting Reality on Compose mode..."):
        code, _, error = Shell.execute(cmd + ["restart"], cwd=ROOT_DIR)
        if (code != 0):
            console.error_panel(error)
            exit(1)

    console.done("Restarted Reality on Compose.")

@app.command("status")
def status_command():
    """To be implemented."""
    services = client.services.list()
    containers = client.containers.list()

    services_table = Table(headers=Columns(["Name", "Containers", "Ports"]),title="Services")
    containers_table = Table(headers=Columns(["Name", "Service", "Status", "Ports"]), title="Containers")

    for service in services:
        services_table.add_row(service.name, len(service.tasks), "---")

    for container in containers:
        containers_table.add_row(container.name, container.labels["com.docker.compose.service"], container.status, "---")

    console.print(services_table)
    console.print(containers_table)

@app.command("logs")
def logs_command():
    """To be implemented."""
    pass