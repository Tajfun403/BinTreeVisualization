# Binary Tree Visualization

A modern C# / WPF application that lets you visualize basic operations on BST and AVL trees.

# Operations supported
Supports:
- Insertion
- Deletion
- Search

All steps performed during an algorithm's execution are described in the log, and all nodes associated with the particular sub-operation are highlighted - what enables clear and easy following of an algorithm.

![Rotation screenshot](https://i.imgur.com/lVTFZxL.png)

Additionally, the amount of performed comparisons and traversals is recorded and can be viewed in form of a graph (utilizes ScottPlott library):
![Screenshot showing graphs](https://i.imgur.com/JnaB9QW.png)

This allows one to verify the algorithms' time complexity with real-world data.

# Quality of Lifes
- Use "Insert 20 items" button to insert twenty random items into the tree instantly. Count can be decreased / increased with `F2` and `F3` keys respectively. 
- Press `Escape` to cancel the current animation and proceed to the final result instantly
- Press `Enter` to repeat the last operation
- Click a node to quickly select it
- Graph's data updates instantly as you perform new operations
- The tree is auto-layouted using a modified Wetherell and Shannon algorithm for visually pleasant and efficient use of space 

# Build
Uses .NET 9.0 and C# 13 preview with WPF UI. To build with Visual Studio 2022.