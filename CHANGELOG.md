# Changelog

## Version Table

- [V0.1a](#version-01a)
- [V0.2a](#version-02a)
- [V0.2.1a](#version-021a)
- [V0.3a](#version-03a)
- [V0.3.1a](#version-031a)

## Version 0.1a

> Added
- Engine
- MiniMax Search function
- Alpha-Beta Pruning

## Version 0.2a

> Added
- Move Ordering
- Quiescence Search
- Transposition Tables

> Changed
- Evaluation function is now perspective-based
- Cleaner code for Graphics

## Version 0.2.1a

> Changed
- Fixed a bug with Search & TT
- TT is no longer used in search if plyFromRoot is 0

## Version 0.3a

> Added
- Multithreading

> Changed
- Engine now uses a separate thread for search
- Engine class is no longer static
- Engine no longer uses simplified threefold checks if TT data cannot be found

## Version 0.3.1a

> Added
- Iterative Deepening

> Changed
- Engine now uses Iterative Deepening search

## Version 0.4a

> Added
- Threaded search timeout
- UI
- Custom position loading

> Changed
- Engine now has a search time limit
- Position can now be loaded at runtime with a valid FEN string