from rich import console
from rich.panel import Panel

class Console(console.Console):
    def __init__(self, *args, **kwargs) -> None:
        super().__init__(*args, **kwargs)

        self.texts = {
            "welcome": "Welcome to [magenta]rctl[/magenta]!",

            "docker_not_found": "Docker is not installed. Please install it and try again.",
            "docker_compose_not_found": "Docker Compose is not installed. Please install it and try again.",
            "kubectl_not_found": "Kubectl was not found. Please install it and try again.",
        }

    def welcome(self) -> None:
        self.print(self.texts["welcome"], style="bold blue")

    def info(self, message: str) -> None:
        self.log(message, style="bold blue")

    def done(self, message: str) -> None:
        self.log(message, style="bold green")

    def warn(self, message: str) -> None:
        self.log(message, style="bold dark_orange3")

    def error(self, message: str) -> None:
        self.log(message, style="bold red")

    def error_panel(self, message: str) -> None:
        panel = Panel(message, title="Error", border_style="red")
        self.print(panel)

    def debug(self, message: str) -> None:
        self.log(message, style="bold magenta")


    def alert_docker_not_found(self) -> None:
        self.error(self.texts["docker_not_found"])

    def alert_docker_compose_not_found(self) -> None:
        self.error(self.texts["docker_compose_not_found"])

    def alert_kubectl_not_found(self) -> None:
        self.error(self.texts["kubectl_not_found"])

console = Console(log_path=False)