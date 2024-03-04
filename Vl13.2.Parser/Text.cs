namespace Vl13._2.Parser;

public static class Text
{
    // ReSharper disable ConvertToConstant.Global
    public static readonly string SimpleProgram =
        """
        include 'main'

        func main -> none {
            startTime : i64 = Vl13._2.VlRuntimeHelper.Time()
        
            for i : f64 = 0.0; i < 10.01; i = i + 1.0 {
                q : f64 = sqrt(i)
                System.Console.WriteLine.f64(q)
                System.Console.WriteLine.f64(q * q)
                
                q = sqrtRec(i, i)
                System.Console.WriteLine.f64(q)
                System.Console.WriteLine.f64(q * q)
                
                System.Console.WriteLine()
                System.Console.WriteLine()
            }
            
            System.Console.WriteLine.i64(Vl13._2.VlRuntimeHelper.Time() - startTime)
        }

        func sqrt x : f64 -> f64 {
            result : f64 = x
        
            while result * result - x >= 0.000000000000001 =>
                result = 0.5 * (result + (x / result))
                
            ret result
        }

        func sqrtRec a : f64, x : f64 -> f64 {
            if x * x - a < 0.000000000000001 => 
                ret x
            
            ret sqrtRec(a, 0.5 * (x + (a / x))) 
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