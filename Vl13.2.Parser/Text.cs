namespace Vl13._2.Parser;

public static class Text
{
    // ReSharper disable ConvertToConstant.Global
    public static readonly string SimpleProgram =
        """
        include 'main'
        include '../../../../StandardLibrary/bin/Debug/net8.0/StandardLibrary.dll'
                 

        func main -> none {
            ptr : i64 = Std.Calloc.i32(123)
            
            for i : i64 = 0; i < 100; i = i + 1 {
                (ptr + i * 8) <- Std.RndInt.i64i64(10, 20)
            }
            
            for i = 0; i < 100; i = i + 1 {
                Std.Write.i64(i)
                Std.Write.char(58)
                Std.Write.char(32)
                Std.WriteLine.i64(*(ptr + i * 8))
            }
            
            Std.Free.i64(ptr)
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