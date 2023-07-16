import subprocess
from typing import List, Tuple

class Shell:
    @staticmethod
    def execute(cmd: List[str], *args, cwd: str = None) -> Tuple[int, str, str]:
        step = subprocess.Popen(cmd, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        step.wait()
        output, error = step.communicate(args)
        return step.returncode, output, error