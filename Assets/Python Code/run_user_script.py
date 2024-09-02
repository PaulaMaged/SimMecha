import os
import importlib.util
import importlib
import sys


# Get the directory of the current executable
def get_script_path():
    """Return the path to user_script.py based on whether running as an executable or in development."""
    if getattr(sys, 'frozen', False):
        # If running as a bundled executable
        base_path = sys._MEIPASS
    else:
        # Running in a development environment
        base_path = os.path.dirname(__file__)

    return os.path.join(base_path, "user_script.py")


def load_script():
    user_script_path = get_script_path()

    try:
        if os.path.exists(user_script_path):
            # Clear the cached module if it was previously loaded
            if "user_script" in sys.modules:
                del sys.modules["user_script"]

            # Dynamically load and execute the external Python file
            spec = importlib.util.spec_from_file_location("user_script", user_script_path)
            user_script = importlib.util.module_from_spec(spec)
            spec.loader.exec_module(user_script)
            print("User script loaded successfully.")
            return user_script  # Return the loaded module
        else:
            print(f"User script not found at {user_script_path}")
            return None  # Return None if script not found
    except Exception as e:
        print(f"Error loading user script: {e}")
        return None  # Return None in case of an exception