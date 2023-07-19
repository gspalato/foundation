from os import path

FILE_DIR = path.dirname(path.abspath(__file__))
FCTL_DIR = path.dirname(FILE_DIR)
TOOLS_DIR = path.dirname(FCTL_DIR)
ROOT_DIR = path.dirname(TOOLS_DIR)
SRC_DIR = path.join(ROOT_DIR, "src")