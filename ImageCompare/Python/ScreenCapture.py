import time
from PIL import ImageGrab
import datetime
import os
import sys
import signal

def capture_screenshots(save_path, interval=0.01):
    if not os.path.exists(save_path):
        os.makedirs(save_path)

    def save_screenshot():
        timestamp = datetime.datetime.now().strftime("%Y-%m-%d_%H-%M-%S-%f")
        filename = f'screenshot_{timestamp}.png'
        full_path = os.path.join(save_path, filename)
        screen = ImageGrab.grab()
        screen.save(full_path, 'PNG')
        print(f'Screenshot saved as {full_path}')

    def signal_handler(sig, frame):
        print("Signal received, saving last screenshot...")
        save_screenshot()
        sys.exit(0)

    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)

    try:
        while True:
            save_screenshot()
            time.sleep(interval)
    except KeyboardInterrupt:
        save_screenshot()
        print("Stopped by user.")

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python ScreenCapture.py <save_path>")
        sys.exit(1)
    save_path = sys.argv[1]
    capture_screenshots(save_path)
