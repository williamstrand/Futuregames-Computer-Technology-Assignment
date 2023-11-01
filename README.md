# Space-Shooter-FG-WSP
 
## Asteroids:
### Before optimization:
The script that is the slowest is the asteroid script. In the game a lot of asteroids are spawned and each of them has a script attached to it that handles it's movement and rotation.

- Asteroid script execution time is around .14 ms.
- Asteroid Spawner script execution time is around .13 ms on a frame when spawning an asteroid. 0 ms on other frames.
- Total execution time for both is around .27 ms on frames when spawning an asteroid. .14 ms on other frames.

Max amount of asteroids were 200.

### After optimization:
By using the burst compiler to calculate the movement and rotation of the asteroids and then using the jobs system to apply the changes, the total execution time of all the asteroids was reduced. The calculation of movement and rotation was moved to the Asteroid Spawner script.

- Asteroid script execution time is around .02 ms.
- Asteroid Spawner script execution time is around .17 ms on a frame when spawning an asteroid. .04 ms on other frames.
- Total execution time for both is around .19 ms on a frame when spawning an asteroid. .06 ms on other frames.

By adding a pooling system to the asteroids the time it takes to spawn an asteroid got reduced to almost 0 ms. This resulted in that the total execution time for both the asteroids and the spawner is constantly around .06 ms.

### Result:
Execution time for asteroids when spawning an asteroid went from .27 ms to .06 ms.
Execution time for asteroids on other frames went from .14 ms to .06 ms.
Pool size was 200.

## Enemies:
The same optimizations were also applied to the enemies.

### Before optimization:
- Enemy scripts execution time is around .03 ms.
- Enemy Spawner script execution time is around .13 ms on a frame when spawning an enemy. 0 ms on other frames.
- Total execution time for both is around .16 ms on a frame when spawning an enemy. .03 ms on other frames.

Max amount of enemies were 10.

### After optimization:
- Enemy scripts execution time is around .0 ms.
- Enemy Spawner script execution time is around .02 ms on all frames.
- Total execution time for both is around .02 ms on all frames

Pool size was 10.

### Result:
Execution time for enemies when spawning an enemy went from .16 ms to .02 ms.
Execution time for asteroids on other frames went from .03 ms to .02 ms.
Since the pool size was so small there was only a very small performance increase.


