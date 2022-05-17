namespace HandwritingVR
{
    public class Evaluation
    {
        public string[] phrases;
        public char[] recognizedChars;
        public int misclassifiedChars;
        public float time; // For words per minute
        public int numberOfWords; // get from phrases
        public int numberOfChars;
        public int countBackspace; // == misclassified
        public char[][] bestmatches; // for each letter
        public string enteredText; // To compare with phrases for final count of how many letters were wrongly recognized

        // Folder student1
        // --> File with raw drawing data
        // --> File Testphrases (Use this to calculate Words-Per-Minute, count letters)
		// --> File with timestamps per phrase + total time
        // --> File with recognized character 
        //             + Top five best matches
		// Backspace counter?
        // my watch fell in the water
        // m
        // m, w, n, 
        // x
        // y, g, v, l,
        
        
        // or
        
        // Expected: m
        // Best Match: m
        // Top five: m, w, ...
        
        // Expected: y 
        // Best Match: g
        // Top five: g, y, x, q, f
        // Corrected: yes
        
        // Expected: " "
        // Found: " "
        
        // Expected: w
        // Best Match: m
        // Top five: m, x, y, z, k
        // Corrected: Not possible
        // Try again: (if found:)
        // Best Match: m
        // Top five: m, w, x, ...
        // Corrected: yes
        
        // Try again: (if not found:)
        // Best Match: m
        // Top five: m, y, x, ...
        // Corrected: not possible Go to next letter
        
    }
}