# Space-Shooter-FG-WSP
 
## Asteroids:
### Before optimization:
The script that is the slowest is the asteroid script. In the game a lot of asteroids are spawned and each of them has a script attached to it that handles it's movement.

- Asteroid script execution time is around .80 ms.
- Asteroid Spawner script execution time is around .14 ms on a frame when spawning an asteroid. 0 ms on other frames.
- Total execution time for both is around .94 ms on frames when spawning an asteroid. .80 ms on other frames.

### After optimization:
By using the burst compiler to calculate the movement of the asteroids and then using the jobs system to apply the change in position the total execution time of all the asteroids was reduced. The calculation of the movement was moved to the Asteroid Spawner script.

- Asteroid script execution time is around 0 ms.
- Asteroid Spawner script execution time is around .70 ms on a frame when spawning an asteroid. .56 ms on other frames.
- Total execution time for both is around .70 ms on a frame when spawning an asteroid. .56 ms on other frames.

By adding a pooling system to the asteroids the time it takes to spawn an asteroid got reduced to almost 0 ms. This resulted in that the total execution time for both the asteroids and the spawner is constantly around .56 ms.

### Result:
Execution time for asteroids when spawning an asteroid were reduced by .36 ms which is a 64% decrease in time.
Execution time for asteroids on other frames were reduced by .24 ms which is a 57.5% decrease in time.

## Enemies:
The same optimizations were also applied to the enemies.

### Before optimization:
- Enemy scripts execution time is around .13 ms.
- Enemy Spawner script execution time is around .14 ms on a frame when spawning an asteroid. 0 ms on other frames.
- Total execution time for both is around .27 ms on a frame when spawning an asteroid. .13 ms on other frames.

### After optimization:
- Enemy scripts execution time is around .05 ms.
- Enemy Spawner script execution time is around .09 ms on a frame when spawning an enemy. .09 ms on other frames.
- Total execution time for both is around .14 ms on all frames

### Result:
Execution time for enemies when spawning an enemy were reduced by .14 ms which is a 48% decrease in time.
Execution time for asteroids on other frames were reduced by .24 ms which is a 57.5% decrease in time.
