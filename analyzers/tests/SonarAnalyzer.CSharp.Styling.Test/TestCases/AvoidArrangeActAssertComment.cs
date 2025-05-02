// Arrange

// Act

// Assert

public class TestClass
{
    public void TestMethod()
    {
        // Noncompliant@+1 {{Remove this Arrange, Act, Assert comment.}}
        // Arrange
        int number1 = 2;
        int number2 = 2;

        // Noncompliant@+1 {{Remove this Arrange, Act, Assert comment.}}
        // Act
        int result = number1 + number2;

        // Noncompliant@+1 {{Remove this Arrange, Act, Assert comment.}}
        // Assert
        _ = 4 == result;

        // Noncompliant@+1
        //Arrange
        // Noncompliant@+1
        //                      Act
        // Noncompliant@+1 ^9#27
        /*     * Assert
         */
        // Noncompliant@+1 ^9#32
        /*
         * // Act
         */
        // Noncompliant@+1
        // /* Assert */
        // Noncompliant@+1
        /// Arrange
        // Noncompliant@+1
        /**
         * Arrange
         */
        // Noncompliant@+1
        // Arrange Act Assert

        /*
         * This not an Act comment or even Arrange or Assert
         */

        // Noncompliant@+3 ^9#10
        // Noncompliant@+3 ^9#6
        // Noncompliant@+3 ^9#9
        // Arrange
        // Act
        // Assert

        // Noncompliant@+1
        // Act & Assert

        // Noncompliant@+1
        /// Arrange
        /// Act
        /// Assert
    }

    // Noncompliant@+1 - FP This comment is part of the method declaration
    /*
     * Act
     */
    public void Multiline_Comment(string input)
    {
        // Noncompliant@+1
        /* Arrange */
        int number1 = 2;
        int number2 = 2;

        // Noncompliant@+1 ^9#29
        /*
         * Act
         */
        int result = number1 + number2;

        // Noncompliant@+1 ^9#32
        /*
           Assert
         */
        _ = 4 == result;
    }

    private void AreEqual(object expected, object actual) { }
}

///
/// Assert
///
public class NotATestClassInTestProject
{
    // Noncompliant@+1 - FP This comment is part of the method declaration
    /**
     * Arrange
     */
    public void Method()
    {
        // Noncompliant@+1
        // Arrange
        int number1 = 2;
        int number2 = 2;

        // Noncompliant@+1
        // Act
        int result = number1 + number2;

        // Noncompliant@+1
        // Assert
        _ = 4 == result;
    }
}
