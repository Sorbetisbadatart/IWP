using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float typeWriterSpeed = 50f;
    //[SerializeField] private float textDelaySeconds = 2;

    public bool IsRunning { get; private set; }

    private readonly List<Punctuation> punctuations = new List<Punctuation>()
    {
        new Punctuation(new HashSet<char>(){'.','!','?'}, 0.5f ),
        new Punctuation(new HashSet<char>(){',',';',':'}, 0.3f)
    };

    private Coroutine typingCoroutine;
    private TagParser tagParser;

    private void Start()
    {
        tagParser = new TagParser();
    }

    public void Run(string textToType, TMP_Text textLabel)
    {
        typingCoroutine = StartCoroutine(TypeText(textToType, textLabel));
    }

    public void Stop()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        IsRunning = false;
    }

    private IEnumerator TypeText(string textToType, TMP_Text textLabel)
    {
        IsRunning = true;
        ResetText(textLabel);

        // Parse the text for tags and get character segments
        var characterSegments = tagParser.ParseTextToSegments(textToType);

        float t = 0;
        int charIndex = 0;

        while (charIndex < characterSegments.Count)
        {
            int lastCharIndex = charIndex;

            t += Time.deltaTime * typeWriterSpeed;
            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, characterSegments.Count);

            for (int i = lastCharIndex; i < charIndex; i++)
            {
                bool isLast = i >= characterSegments.Count - 1;

                // Build the text up to current position
                textLabel.text = BuildTextUpToIndex(characterSegments, i);

                // Handle punctuation delays
                char currentChar = characterSegments[i].Character;
                if (IsPunctuation(currentChar, out float waitTime) && !isLast &&
                    !IsPunctuation(characterSegments[i + 1].Character, out _))
                {
                    yield return new WaitForSeconds(waitTime);
                }
            }

            yield return null;
        }

        IsRunning = false;
    }

    private string BuildTextUpToIndex(List<CharacterSegment> segments, int currentIndex)
    {
        var finalBuilder = new StringBuilder();
        var activeTagStack = new Stack<RichTextTag>();

        for (int i = 0; i <= currentIndex; i++)
        {
            var segment = segments[i];

            if (segment.IsOpeningTag)
            {
                finalBuilder.Append(segment.Tag.OpenTag);
                activeTagStack.Push(segment.Tag);
            }
            else if (segment.IsClosingTag)
            {
                if (activeTagStack.Count > 0)
                {
                    var tag = activeTagStack.Pop();
                    finalBuilder.Append(tag.CloseTag);
                }
            }
            else
            {
                // Just append the character - tags are already open in the string
                finalBuilder.Append(segment.Character);
            }
        }

        // Close any remaining open tags at the end
        while (activeTagStack.Count > 0)
        {
            finalBuilder.Append(activeTagStack.Pop().CloseTag);
        }

        return finalBuilder.ToString();
    }
    private bool IsPunctuation(char character, out float waitTime)
    {
        foreach (Punctuation punctuationCategory in punctuations)
        {
            if (punctuationCategory.Punctuations.Contains(character))
            {
                waitTime = punctuationCategory.WaitTime;
                return true;
            }
        }
        waitTime = default;
        return false;
    }

    private void ResetText(TMP_Text textLabel)
    {
        textLabel.text = string.Empty;
    }

    // Supporting classes for tag parsing
    [System.Serializable]
    private class TagParser
    {
        private readonly Dictionary<string, RichTextTag> supportedTags = new Dictionary<string, RichTextTag>()
        {
            { "b", new RichTextTag("b", "</b>") },
            { "i", new RichTextTag("i", "</i>") },
            { "u", new RichTextTag("u", "</u>") },
            { "color", new RichTextTag("color", "</color>") },
            { "size", new RichTextTag("size", "</size>") }
        };

        public List<CharacterSegment> ParseTextToSegments(string text)
        {
            var segments = new List<CharacterSegment>();
            var currentIndex = 0;

            while (currentIndex < text.Length)
            {
                if (text[currentIndex] == '<')
                {
                    int tagEnd = text.IndexOf('>', currentIndex);
                    if (tagEnd == -1) break;

                    string tagContent = text.Substring(currentIndex + 1, tagEnd - currentIndex - 1);

                    if (tagContent.StartsWith("/"))
                    {
                        // Closing tag
                        string tagName = tagContent.Substring(1).Split('=')[0].Trim();
                        if (supportedTags.ContainsKey(tagName))
                        {
                            segments.Add(new CharacterSegment(supportedTags[tagName], true, false));
                        }
                    }
                    else
                    {
                        // Opening tag
                        string tagName = tagContent.Split('=')[0].Trim();
                        if (supportedTags.ContainsKey(tagName))
                        {
                            // Handle tags with attributes like <color=red>
                            string fullOpenTag = $"<{tagContent}>";
                            var tag = new RichTextTag(tagName, supportedTags[tagName].CloseTag, fullOpenTag);
                            segments.Add(new CharacterSegment(tag, false, true));
                        }
                    }

                    currentIndex = tagEnd + 1;
                }
                else
                {
                    // Regular character
                    segments.Add(new CharacterSegment(text[currentIndex]));
                    currentIndex++;
                }
            }

            return segments;
        }
    }

    [System.Serializable]
    private struct CharacterSegment
    {
        public char Character { get; }
        public RichTextTag Tag { get; }
        public bool IsOpeningTag { get; }
        public bool IsClosingTag { get; }
        public bool IsCharacter => !IsOpeningTag && !IsClosingTag;

        public CharacterSegment(char character)
        {
            Character = character;
            Tag = default;
            IsOpeningTag = false;
            IsClosingTag = false;
        }

        public CharacterSegment(RichTextTag tag, bool isClosingTag, bool isOpeningTag)
        {
            Character = '\0';
            Tag = tag;
            IsOpeningTag = isOpeningTag;
            IsClosingTag = isClosingTag;
        }
    }

    [System.Serializable]
    private struct RichTextTag
    {
        public string Name { get; }
        public string OpenTag { get; }
        public string CloseTag { get; }

        public RichTextTag(string name, string closeTag) : this(name, closeTag, $"<{name}>")
        {
        }

        public RichTextTag(string name, string closeTag, string openTag)
        {
            Name = name;
            OpenTag = openTag;
            CloseTag = closeTag;
        }
    }

    private readonly struct Punctuation
    {
        public readonly HashSet<char> Punctuations;
        public readonly float WaitTime;

        public Punctuation(HashSet<char> punctuations, float waitTime)
        {
            Punctuations = punctuations;
            WaitTime = waitTime;
        }
    }
}