# ProceduralGenerationSample
A project describing a simple but effective 2D dungeon procedural generation.

It's based on a method used by TinyKeep's developer and described in https://www.reddit.com/r/gamedev/comments/1dlwc4/procedural_dungeon_generation_algorithm_explained/ .
Also, the method is detailed in https://github.com/adonaac/blog/issues/7
This is nothing more but an Unity's script implementation of the described method.

To test it, just open the project in Unity 5.x+, open the scene Scenes/ProceduralGenerationTest and run the sample (don't forget to enable gizmos in order to visualize the step by step result :) ).
It will only generate the rectangles representing the generated dungeon, then it's up to you to use them as you wish, so some code proficiency is required (In [Behemutt's project](http://manaspark.behemutt.com/) [Mana Spark](http://manaspark.behemutt.com/), I just get those rects, assume each unit of their dimensions represents a tile and, then, I populate my level tile by tile).

I don't know if I will update this project in the future, but it will remain here for public reference :). Anyway, feel free to contact me 
[@doliveira87](https://twitter.com/doliveira87)!

I hope it may prove to be useful for someone!

Best regards,
Douglas L. Oliveira