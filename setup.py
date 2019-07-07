# Here we imported the 'setup' module which allows us to install Python scripts to the local system beside performing some other tasks, you can find the documentation here: https://docs.python.org/2/distutils/apiref.html 
from distutils.core import setup 

setup(name = "tubecatcher", # Name of the program. 
      version = "1.0", # Version of the program. 
      description = "An easy-to-use web interface to create & share pastes easily", # You don't need any help here. 
      author = "Ibrahim Khalil", # Nor here. 
      author_email = "onucsecu@gmail.com",# Nor here :D 
      url = "http://facebook.com/ibrahim.onu.1", # If you have a website for you program.. put it here. 
      license='GPLv3', # The license of the program. 
      scripts=['tubecatcher'], # This is the name of the main Python script file, in our case it's "myprogram", it's the file that we added under the "myprogram" folder. 

# Here you can choose where do you want to install your files on the local system, the "myprogram" file will be automatically installed in its correct place later, so you have only to choose where do you want to install the optional files that you shape with the Python script 
      data_files = [ ("lib/tubecatcher", ["ui.glade","tc.png","TC.png"]), # This is going to install the "ui.glade" file under the /usr/lib/myprogram path. 
                     ("share/applications", ["tubecatcher.desktop"]) ] ) # And this is going to install the .desktop file under the /usr/share/applications folder, all the folder are automatically installed under the /usr folder in your root partition, you don't need to add "/usr/ to the path. 
