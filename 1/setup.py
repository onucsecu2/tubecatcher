#!/usr/bin/env python

from setuptools import setup

setup(name='tubecatcher',
      version='1.0.1',
      description='Youtube video downloading tools',
      author='Ibrahim Khalil',
      author_email='onucsecu@gmail.com',
      scripts=['tubecatcher/tubecatcher'],
      install_requires=['youtube_dl', 'PyGObject', 'pycairo'],
      data_files = [ ("/home/onu/PycharmProjects/tubecatcher/debian/tubecatcher/usr/lib/tubecatcher", ["data/ui.glade"]),
                     ("/home/onu/PycharmProjects/tubecatcher/debian/tubecatcher/usr/lib/tubecatcher", ["data/tubecatcher.png"]),
                     ("/home/onu/PycharmProjects/tubecatcher/debian/tubecatcher/usr/share/applications", ["data/Tubecatcher.desktop"]) 
                   ] 
    )

   
