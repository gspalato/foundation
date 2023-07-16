import typer
from typer import Typer
from typing_extensions import Annotated

from rctl.__main__ import ROOT_DIR, console
from rctl.checks import Checks
from rctl.shell import Shell

# Create the app.
app: Typer = Typer()

# Add the subcommands.
@app.command("build", help="Builds the container images.")
def build(
    push: Annotated[bool, typer.Option("-p", help="Pushes the images to the registry after building.")] = True
):
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
                console.print_exception(error, style="red")
                exit(1)

    console.done("Built container images on Compose mode.")

@app.command("up", help="Starts Reality on Compose mode.")
def up(
    build: Annotated[bool, typer.Option("-b", help="Builds the images before starting.")] = False,
    restart: Annotated[bool, typer.Option("-r", help="Kills the services before starting.")] = False,
):
    # Check if Docker Compose is installed.
    installed, cmd = Checks.get_docker_compose()
    if (not installed):
        console.alert_docker_compose_not_found()
        exit(1)

    if (restart):
        down()

    if (build):
        build(False)

    with console.status("[bold blue]Starting Reality on Compose mode..."):
        code, _, error = Shell.execute(cmd + ["up", "-d"], cwd=ROOT_DIR)
        if (code != 0):
            console.error("Failed to start Reality on Compose mode.")
            console.console.print_exception(error, style="red")
            exit(1)
    
    console.done("Started Reality on Compose mode.")

@app.command("down", help="Stops Reality on Compose mode and deletes all containers.")
def down():
    # Check if Docker Compose is installed.
    installed, cmd = Checks.get_docker_compose()
    if (not installed):
        console.alert_docker_compose_not_found()
        exit(1)

    with console.status("Stopping Reality on Compose mode..."):
        code, _, error = Shell.execute(cmd + ["down"], cwd=ROOT_DIR)
        if (code != 0):
            console.print_exception(error)
            exit(1)

    console.done("Stopped Reality on Compose.")

@app.command("restart", help="Restarts Reality on Compose mode.")
def restart():
    # Check if Docker Compose is installed.
    installed, cmd = Checks.get_docker_compose()
    if (not installed):
        console.alert_docker_compose_not_found()
        exit(1)

    with console.status("Restarting Reality on Compose mode..."):
        code, _, error = Shell.execute(cmd + ["restart"], cwd=ROOT_DIR)
        if (code != 0):
            console.print_exception(error)
            exit(1)

    console.done("Restarted Reality on Compose.")

@app.command("status")
def status():
    """To be implemented."""
    pass

@app.command("logs")
def logs():
    """To be implemented."""
    pass