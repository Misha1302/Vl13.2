namespace Vl13._2.Parser;

public static class Text
{
    // ReSharper disable ConvertToConstant.Global
    public static readonly string SimpleProgram =
        """
        include 'main'

        struct Vector3 x : i64, y : i64, z : i64, print : i64, add : i64

        func main -> none {
            q : i64 = &createVector
            a : Vector3 = q[i64, i64, i64, Vector3](-3, -9, 127)
            
            w : i64 = &cloneVector
            b : Vector3 = w[&Vector3, Vector3](&a)
            
            e : i64 = &printVec
            
            e[&Vector3, none](&a)
            System.Console.WriteLine()
            e[&Vector3, none](&b)
            
            c : Vector3 = createVector(1,2,3)
            System.Console.WriteLine()
            System.Console.WriteLine.i64(c.x)
            printVec(&c)
        }

        func createVector x : i64, y : i64, z : i64 -> Vector3 {
            a : Vector3
            a.x = x
            a.y = y
            a.z = z
            
            a.print = -111
            a.add = -222
        
        
            ret a
        }

        func cloneVector vec : &Vector3 -> Vector3 {
            a : Vector3
            
            a.x = vec.x
            a.y = vec.y
            a.z = vec.z
            
            a.print = vec.print
            a.add = vec.add
            
            ret a
        }

        func printVec a : &Vector3 -> none {
            System.Console.WriteLine.i64(a.x)
            System.Console.WriteLine.i64(a.y)
            System.Console.WriteLine.i64(a.z)
            System.Console.WriteLine.i64(a.print)
            System.Console.WriteLine.i64(a.add)
        }
        """;

    public static readonly string FullProgram =
        """
        include 'main'

        struct Vector3 x : f64, y : f64, z : f64, print : i64[&Vector3, none], add : i64[&Vector3, &Vector3, Vector3]

        func main -> none {
            vec : Vector3; vec2 : Vector3; vec3 : Vector3
        
            vec.x = 5.54; vec.y = 12.54; vec.z = -32.54; vec.print = &printSquaredVec; vec.add = &addVecs
            vec2.x = 5.54; vec2.y = 12.54; vec2.z = -32.54; vec2.print = &printSquaredVec; vec2.add = &addVecs
        
            // no need to copy
            vec3 = vec.add[&Vector3, &Vector3, Vector3](&vec, &vec2)
            vec3.print[&Vector3, none](&vec3)
        }

        func printSquaredVec vec : &Vector3 -> none {
            System.Console.WriteLine.f64(square(vec.x))
            System.Console.WriteLine.f64(square(vec.y))
            System.Console.WriteLine.f64(square(vec.z))
        }

        func addVecs a : &Vector3, b : &Vector3 -> Vector3 {
            vec : Vector3
        
            vec.x = a.x + b.x; vec.y = a.y + b.y; vec.z = a.z + b.z
            vec.print = a.print; vec.add = a.add
        
            ret vec
        }

        func square x : f64 -> f64 {
            ret x * x
        }
        """;
}