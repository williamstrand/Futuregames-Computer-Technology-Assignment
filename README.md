# Space-Shooter-FG-WSP
 
## Asteroids:
### Before optimization:
The script that is the slowest is the asteroid script. In the game a lot of asteroids are spawned and each of them has a script attached to it that handles it's movement and rotation.

Max amount of asteroids were 200.
- Asteroid script execution time is around .14 ms.
- Asteroid Spawner script execution time is around .13 ms on a frame when spawning an asteroid. 0 ms on other frames.
- Total execution time for both is around .27 ms on frames when spawning an asteroid. .14 ms on other frames.

Max amount of asteroid were 1000.
- Asteroid script execution time is around 1.25 ms.
- Asteroid Spawner script execution time is around .13 ms on a frame when spawning an asteroid. 0 ms on other frames.
- Total execution time for both is around 1.38 ms on frames when spawning an asteroid. 1.25 ms on other frames.

### After optimization:
By using the burst compiler to calculate the movement and rotation of the asteroids and then using the jobs system to apply the changes, the total execution time of all the asteroids was reduced. The calculation of movement and rotation was moved to the Asteroid Spawner script.

Pool size was 200.
- Asteroid script execution time is around .02 ms.
- Asteroid Spawner script execution time is around .17 ms on a frame when spawning an asteroid. .04 ms on other frames.
- Total execution time for both is around .19 ms on a frame when spawning an asteroid. .06 ms on other frames.
By adding a pooling system to the asteroids the time it takes to spawn an asteroid got reduced to almost 0 ms. This resulted in that the total execution time for both the asteroids and the spawner is constantly around .06 ms.

Pool size was 1000.
- Asteroid script execution time is around .14 ms.
- Asteroid Spawner script execution time is around .14 ms on all frames.
- Total execution time for both is around .28 ms on all frames.

## Enemies:
The same optimizations were also applied to the enemies.

### Before optimization:
Max amount of enemies were 10.
- Enemy scripts execution time is around .03 ms.
- Enemy Spawner script execution time is around .13 ms on a frame when spawning an enemy. 0 ms on other frames.
- Total execution time for both is around .16 ms on a frame when spawning an enemy. .03 ms on other frames.

### After optimization:
Pool size was 10.
- Enemy scripts execution time is around .0 ms.
- Enemy Spawner script execution time is around .02 ms on all frames.
- Total execution time for both is around .02 ms on all frames

# Summary
When using the traditional unity approach the game started dipping below 60 fps when there were 1000 spawned asteroids.
By using a Data-oriented approach with the burst compiler and the jobs system from the DOTS package to move and rotate the asteroids, the game was running at around 120 fps with 1000 spawned asteroids.
To use the jobs system you can only use unmanaged resources. This is great for performance since there is less overhead and the garbage collector does not need to keep track of it.
