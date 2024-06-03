This is an experimental program designed to manually check two images, and create a pass, fail, or flag marker after comparing.  
It can do this by a direct compare of two images or it can monitor a directory and check for new images every 5 seconds.  
After 5 seconds it will compare the last image it compared with the latest image in the directory.
After the compare and flag event, it will wait 5 seconds, move the last compare to the current image to be compared and repeat.
