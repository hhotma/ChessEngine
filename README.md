
# Chess Engine

Chess engine with a simple UCI interface that I made as my high school's Maturita project.


## UCI Interface

### Check if UCI interface initiated correctly
```text
    uci
```
### Check if engine is ready
```text
    isready
```
### Initiate a new game
```text
    ucinewgame
```

### Set a starting position (moves is optional)
```text
    position fen rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 moves e2e4 e7e5
```
```text
    position startpos
```

### Get a best move in current position
```text
    go movetime 5000
```
```text
    go wtime 40400 btime 32300 winc 1000 binc 1000
```
#### Params:
    movetime - allowed time to think (milliseconds)
    wtime - white time remaining (milliseconds)
    btime - black time remaining (milliseconds)
    winc - white time increment (milliseconds)
    binc - black time increment (milliseconds)

### Stop the engine in thinking
```text
    stop
```

### Quit the app
```text
    quit
```
