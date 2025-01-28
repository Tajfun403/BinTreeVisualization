# Project overview
## Base BST
The binary search tree is build out of nodes - starting by root, each of the subsequent children can have two children of their own.  
The base property of the binary tree which allows for O(log n) search is the way the children nodes are placed - nodes smaller than their parent are placed on the left, and nodes bigger than their parent - on the right. Thanks to this, the area of tree which needs to be searched is effectively halved with each recursive iterations (assuming the tree is balanced well enough).
### Insertion
- Proceed like with normal search, then if a spot is found where the search would continue but the spot is empty - insert a new node into it.
### Search
- Follow the tree with the principle of going left if the searched value is smaller than the current node, and right if the searched value is bigger. 
- Check if the current is the searched node if neither of those paths can be taken.
### Deletion
- Find an item to delete. 
- Next, try to find its successor - the next element which is minimally smaller than the current one (that can be obtained by going one to right and then max to left, thanks to binary tree properties). 
- Once a successor is found, swap the victim with the succesor, and then remove the victim whose been moved out of the original location. 
- If the victim is not placed in a leaf position (i.e. having no children), repeat finding a successor and swapping until a leaf node is reached which can then be removed safely. 

All operations have O(log n) time complexity, but can grow up to O(1) with an unbalanced tree.

## AVL
AVL tree is a type of a binary search tree. It operates on the same base princinple regarding to the location of nodes, but with an additional mechanic - rotations.  
Whenever a subtree becomes unbalances - i.e. one of its sides is at least two nodes taller than the other one - a node with the middle value is picked out of the long branch, and then the other nodes from this branch are balanced around it.  
Thanks to this, a normally successive chain (eg. 1 -> 2 -> 3) becomes a neatly balanced, smaller tree (with node {2} pointing to {1} to the left and to {3} to the right).

This helps ensure that tree doesn't become unbalanced and its operations do not degrade into O(1) complexity.

Operations on AVL trees start exactly the same way as on base BSTs, but then after a node is either inserted or removed, all parents of the affected node are visitied to make sure the tree is kept in balance, and rotations are performed if they are not.
 
## App structure
The application is written utilizing C# 13 preview language, .NET 9.0 framework, as well as WPF framework for the UI.  
The tree is layouted using a modified Wetherell and Shannon algorithm.  
It uses Material Design (http://materialdesigninxaml.net/) NuGet package for the interface theme, as well as ScottPlot library (https://scottplot.net/) for rendering the collected performance data into graphs.  
The tree's logical data structures are partially separated from the WPF elements and use a composite model.
- `BinTree<T>` and `Node<T>` are the main data logical structures that hold the info about the binary tree and can perform operations on it. 
- `TreeLayout` is a helper class that can auto-layout the tree.
- `BinTreeMan` is a WPF page which holds the main user interface and delegates operations requested by user to the binary tree.
- `BinTreeControl` is a WPF UserControl that holds Canvas where the nodes are physically placed into
- `NodeControl` is a WPF UserControl that represents a visual node - each `NodeControl` is directly managed by a `Node<T>` it is associated with, and physically placed upon `BinTreeControl`'s Canvas
- `NodeArrow` is a WPF UserControl that represents and arrow that can point from one point into another. Each `Node<T>` manages an arrow pointing towards it.
- `ProgressLabel` is a WPF Control that is used for displaying descriptions.
- `TreeStats` and `StatsWindow.xaml` are classes used for gathering and displaying the statistics respectively.

Documentation PDF is generated with Doxygen.  
Detailed descriptions of each of the classes, methods, and properties used in the program are contained in the following chapters of the documentation PDF.