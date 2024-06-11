import time
from PIL import ImageGrab
import datetime
import os
def capture_screenshots(interval=0.01):  
    save_path = f"C:\Sandbox\Python\ScreenCapture\Capture"
    # Check if the directory exists, if not, create it
    if not os.path.exists(save_path):
        os.makedirs(save_path)
    try:
        while True:
            # Current time
            timestamp = datetime.datetime.now().strftime("%Y-%m-%d_%H-%M-%S-%f")
            filename = f'screenshot_{timestamp}.png'
            full_path = os.path.join(save_path, filename)
            # Screenshot
            screen = ImageGrab.grab()
            screen.save(full_path, 'PNG')
            print(f'Screenshot saved as {full_path}')
            time.sleep(interval)
    except KeyboardInterrupt:
        timestamp = datetime.datetime.now().strftime("%Y-%m-%d_%H-%M-%S-%f")
        filename = f'screenshot_{timestamp}.png'
        full_path = os.path.join(save_path, filename)
        # Screenshot
        screen = ImageGrab.grab()
        screen.save(full_path, 'PNG')
        print(f'Screenshot saved as {full_path}')
        print("Stopped by user.")
        
if __name__ == "__main__":
    capture_screenshots()