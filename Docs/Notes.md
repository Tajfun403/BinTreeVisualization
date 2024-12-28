# General assignment
Make a visualization of basic operations on BST and AVL trees using any programming language and UI framework.
Operations to support:
- Search
- Insert
- Delete
- Find min
- Find max

# Roadmap
Project requirements:
- [ X ] Initial UI setup
- [ X ] First block spawned
- [ X ] Setup text output
- [ X ] Setup animations
- [ X ] Setup arrows
- [ X ] Fill in algorithms
- [ X ] Make auto layout

Additional subobjectives:
- [ X ] Add more UI controls
- [ X ] Add statistics on operation count
- [ X ] Scale tree to fit automatically
- [   ] Add Red Black Trees?
- [   ] Save / load trees?
- [   ] Add tests

# AVL tree in general
A nice writeup on AVL trees: https://simpledevcode.wordpress.com/2014/09/16/avl-tree-in-c/

It is mostly a BST tree with added rotations, so a single base class with a `bool bDoRotations` should suffice.

# WPF specifics
- Arrows do not exist per-se as a WPF object. Can make a container with several lines to mimic them:
  https://stackoverflow.com/a/68552890
- For centering nodes:
  - Grid supports putting its items in location relative to center. Canvas do not respect that by default.
  - Can work around that by putting Canvas in a Grid - its reflectiveness seems to be inherited.
- Animating locations of custom arrows require a DependencyProp, which then can be fed into PointAnimatiom: 
  - https://learn.microsoft.com/en-us/dotnet/desktop/wpf/properties/dependency-properties-overview?view=netdesktop-9.0
  - https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/how-to-animate-the-position-of-an-object-by-using-pointanimation
- Auto-scaling canvas should be possible with a ViewBox, but it clears out entire view on use?
  - Canvas do support transforms, so can just apply ScaleTransformation dynamically

# Layouting nodes
- Layouting nodes of a binary tree in such a way so that neither space is wasted nor the nodes overlay seem to be harder than expected. Might shelve it for now.
- Found suggestion: Reignold-Tilford's algorithm - but it is for normal trees and not binary ones
- A working algorithm from 1981: https://reingold.co/tidier-drawings.pdf 
  (Tidier Drawing of Trees by Edward M. Reingold and John S. Tilford (IEEE Transactions on Software Engineering, Volume 7, Number 2, March 1981))
- This paper also references an easier Wetherell and Shannon algorithm, which has some flaws (as highlighted in the paper), but it also easier to implement
  - Its cons seem to be insignificant enough for this application
  - Also highlighted here: https://willrosenbaum.com/assets/pdf/2023s-cosc-225/tidy-drawings-of-trees.pdf