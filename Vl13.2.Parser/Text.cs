namespace Vl13._2.Parser;

public static class Text
{
    // ReSharper disable ConvertToConstant.Global
    public static readonly string SimpleProgram =
        """
        include 'main'
        
        struct Vector3 x : i64, y : i64, z : i64, print : i64, add : i64

        func main -> none {
            a : Vector3 = createVector()
        
            System.Console.WriteLine.i64(a.x)
            System.Console.WriteLine.i64(a.y)
            System.Console.WriteLine.i64(a.z)
            System.Console.WriteLine.i64(a.print)
            System.Console.WriteLine.i64(a.add)
        }
        
        func createVector -> Vector3 {
            a : Vector3
            a.x = 23
            a.y = 43
            a.z = a.x * a.y * -2
            
            a.print = -111
            a.add = -222
        
        
            ret a
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