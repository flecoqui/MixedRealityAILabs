    using System.Net; 
     using System.Threading.Tasks; 
 
 
    public static int Run(CustomObject req, TraceWriter log) 
     {    
         Random rnd = new Random(); 
         int randomInt = rnd.Next(0, 3); 
         return randomInt; 
     } 
 
 
    public class CustomObject  
     { 
          public String name {get; set;} 
     } 