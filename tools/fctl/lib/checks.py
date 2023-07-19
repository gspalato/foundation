from typing import List, Tuple

from lib.directories import ROOT_DIR
from lib.shell import Shell

class Checks:
    @staticmethod
    def get_docker() -> Tuple[bool, List[str]]:
        """ Checks if Docker is installed. Returns a bool indicating if it's installed and the command to use it."""
        import shutil

        path = shutil.which("docker")
        if (path):
            return True, ["docker"]
        else:
            return False, []

    @staticmethod
    def get_docker_compose() -> Tuple[bool, List[str]]:
        """ Checks if Docker Compose is installed. Returns a bool indicating if it's installed and the command to use it."""
        import shutil

        # First check for an older docker-compose command.
        path = shutil.which("docker-compose")
        if (path):
            return True, ["docker-compose"]
        
        docker_exists, _ = Checks.get_docker()
        if (not docker_exists):
            return False, []

        # Then, check for the newer docker compose command.
        try:
            code, _, _ = Shell.execute(["docker", "compose", "version"], cwd=ROOT_DIR)
            if (code == 0):
                return True, ["docker", "compose"]
        except Exception:
            pass

        return False, []
    
    @staticmethod
    def check_kubernetes(cmd: List[str]) -> Tuple[bool, List[str]]:
        """ Checks if Kubectl is installed. Returns a bool indicating if it's installed and the command to use it."""
        try:
            code, _, _ = Shell.execute(cmd + ["version"], cwd=ROOT_DIR)
            if (code == 0):
                return True, cmd
            else:
                return False, []
        except Exception:
            pass

        return False, []