﻿// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

/*
 
include 'std.dll' as std

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
    std.printi64(square(vec.x))
    std.printi64(square(vec.y))
    std.printi64(square(vec.z))
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

*/