from rich.console import Console

class Dialogs:
    console = Console()

    def __init__(self) -> None:
        self.texts = {
            "welcome": "Welcome to the Reality Deploy Tool!",

            "docker_not_found": "Docker is not installed. Please install it and try again.",
            "docker_compose_not_found": "Docker Compose is not installed. Please install it and try again.",
            "kubectl_not_found": "Kubectl was not found. Please install it and try again.",
        }

    def welcome(self) -> None:
        self.console.log(self.texts["welcome"], style="bold blue")

    def info(self, message: str) -> None:
        self.console.log(message, style="bold blue")

    def done(self, message: str) -> None:
        self.console.log(message, style="bold green")

    def warn(self, message: str) -> None:
        self.console.log(message, style="bold yellow")

    def error(self, message: str) -> None:
        self.console.log(message, style="bold red")

    def debug(self, message: str) -> None:
        self.console.log(message, style="bold magenta")

    def log(self, message: str) -> None:
        self.console.log(message)


    def alert_docker_not_found(self) -> None:
        self.error(self.texts["docker_not_found"])

    def alert_docker_compose_not_found(self) -> None:
        self.error(self.texts["docker_compose_not_found"])

    def alert_kubectl_not_found(self) -> None:
        self.error(self.texts["kubectl_not_found"])

    