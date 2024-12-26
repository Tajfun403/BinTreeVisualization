# General assignment
Make a visualization of basic operations on BST and AVL trees using any programming language and UI framework.
Operations to support:
- Search
- Insert
- Delete
- Find min
- Find max

# Roadmap
- [ X ] Initial UI setup
- [ X ] First block spawned
- [ X ] Setup text output
- [ X ] Setup animations
- [ X ] Setup arrows
- [   ] Fill in algorithms
- [   ] Add stats summaries

# AVL tree in general
A nice writeup on AVL trees: https://simpledevcode.wordpress.com/2014/09/16/avl-tree-in-c/

It is mostly a BST tree with added rotations, so can make a single base class.

# WPF specifics
- Arrows do not exist per-se as a WPF object. Can make a container with several lines to mimic them:
  https://stackoverflow.com/a/68552890
- For centering nodes:
  - Grid supports putting its items in location relative to center. Canvas do not respect that by default.
  - Can work around that by putting Canvas in a Grid - its reflectiveness seems to be inherited.
- Animating locations of custom arrows require a DependencyProp, which then can be fed into PointAnimatiom: 
  - https://learn.microsoft.com/en-us/dotnet/desktop/wpf/properties/dependency-properties-overview?view=netdesktop-9.0
  - https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/how-to-animate-the-position-of-an-object-by-using-pointanimation

# Layouting nodes
- Layouting nodes of a binary tree in such a way so that neither space is wasted nor the nodes overlay seem to be harder than expected. Might shelve it for now.
- Some suggestions start with Reignold-Tilford's algorithm