namespace Vl13._2.Parser;

public static class Text
{
    // ReSharper disable ConvertToConstant.Global

    public static readonly string SimpleProgram =
        """
        include 'main'

        func main -> none {
            i : i64 = -123
            System.Console.WriteLine.i64(i)
            i = 0
            System.Console.WriteLine.i8(i)
            i = 45 // 45
            i = i - 100 // -55
            i = i * -2 // 110
            System.Console.WriteLine.i32(i % 3) // 2
            ret
        }
        """;

    public static readonly string FullProgram =
        """
        include 'main'

        struct Vector3 x : i64, y : i64, z : i64, print : i64, add : i64

        func main -> none {
            vec : Vector3; vec2 : Vector3; vec3 : Vector3
        
            vec.x = 5; vec.y = 12; vec.z = -32; vec.print = &printVec; vec.add = &addVecs
            vec2.x = 5; vec2.y = 12; vec2.z = -32; vec2.print = &printVec; vec2.add = &addVecs
        
            // no need to copy
            vec3 = (vec.add)(&vec, &vec2)
            (vec3.print)(&vec3)
        }

        func printSquaredVec vec : &Vector3 -> none {
            System.Console.WriteLine.i64(square(vec.x))
            System.Console.WriteLine.i64(square(vec.y))
            System.Console.WriteLine.i64(square(vec.z))
        }

        func addVecs a : &Vector3, b : &Vector3 -> Vector3 {
            vec : Vector3
        
            vec.x = a.x + b.x; vec.y = a.y + b.y; vec.z = a.z + b.z
            vec.print = a.print; vec.add = a.add
        
            ret vec
        }

        func square x : i64 -> i64 {
            ret x * x
        }
        """;
}