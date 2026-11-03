using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class RandomReferenceGenerator : IReferenceGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private readonly IRandomProvider _randomProvider;

    public RandomReferenceGenerator(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
    }

    public string Generate()
    {
        char[] characters = new char[8];

        for (int i = 0; i < characters.Length; i++)
        {
            int index = _randomProvider.Next(Alphabet.Length);
            characters[i] = Alphabet[index];
        }

        return new string(characters);
    }
}