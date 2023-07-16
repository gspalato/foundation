from docker.errors import APIError
from rich.prompt import Prompt

from lib.console import console
from lib.shell import Shell

from platforms.compose.api import client

class Utils:
    @staticmethod
    def login_to_registry(host: str = None) -> str:
        """ Logs into the Docker registry. Returns the username used to login. """

        registry_name = host or "Docker Hub"

        # Login to registry
        if (host is None):
            console.info("No registry host specified. Using Docker Hub.")
        else:
            console.info("Using registry host at " + host + ".")

        username = Prompt.ask("Enter your registry username: ", show_default=False)
        password = Prompt.ask("Enter your registry password: ", show_default=False, password=True)

        with console.status("[bold blue]Logging in to registry...") as status:
            try:
                client.login(username, password, registry=host)
            except APIError as error:
                console.error("Failed to login to registry.")
                console.error_panel(error.explanation)
                exit(1)
        
        if (host is None):
            console.done("Logged in to Docker Hub.")
        else:
            console.done("Logged in to registry at " + registry_name + ".")

        return username
