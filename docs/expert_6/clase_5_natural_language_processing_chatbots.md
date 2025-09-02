# 💬 **Clase 5: Natural Language Processing y Chatbots**

## 🎯 **Objetivos de la Clase**
- Implementar procesamiento de lenguaje natural avanzado
- Crear chatbots inteligentes con .NET
- Analizar conversaciones y diálogos
- Integrar servicios de IA conversacional

## 📚 **Contenido Teórico**

### **1. Fundamentos de NLP con .NET**

#### **Procesamiento de Texto Avanzado**
```csharp
public class AdvancedTextProcessor
{
    private readonly MLContext _mlContext;
    private readonly ILogger<AdvancedTextProcessor> _logger;

    public AdvancedTextProcessor(ILogger<AdvancedTextProcessor> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _logger = logger;
    }

    public async Task<ProcessedText> ProcessTextAsync(string text)
    {
        try
        {
            var result = new ProcessedText
            {
                OriginalText = text,
                Tokens = TokenizeText(text),
                LemmatizedTokens = LemmatizeTokens(TokenizeText(text)),
                NamedEntities = ExtractNamedEntities(text),
                Sentiment = AnalyzeSentiment(text),
                Language = DetectLanguage(text),
                Intent = ClassifyIntent(text),
                Entities = ExtractEntities(text)
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing text: {Text}", text);
            throw;
        }
    }

    private IEnumerable<string> TokenizeText(string text)
    {
        // Tokenización básica
        return text.Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?' }, 
            StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length > 1);
    }

    private IEnumerable<string> LemmatizeTokens(IEnumerable<string> tokens)
    {
        // Lemmatización simplificada
        var lemmatizedTokens = new List<string>();
        
        foreach (var token in tokens)
        {
            var lemmatized = token.ToLower();
            
            // Reglas básicas de lemmatización
            if (lemmatized.EndsWith("ing"))
            {
                lemmatized = lemmatized.Substring(0, lemmatized.Length - 3);
            }
            else if (lemmatized.EndsWith("ed"))
            {
                lemmatized = lemmatized.Substring(0, lemmatized.Length - 2);
            }
            else if (lemmatized.EndsWith("s") && lemmatized.Length > 3)
            {
                lemmatized = lemmatized.Substring(0, lemmatized.Length - 1);
            }
            
            lemmatizedTokens.Add(lemmatized);
        }
        
        return lemmatizedTokens;
    }

    private IEnumerable<NamedEntity> ExtractNamedEntities(string text)
    {
        var entities = new List<NamedEntity>();
        
        // Detectar emails
        var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
        var emailMatches = Regex.Matches(text, emailPattern);
        foreach (Match match in emailMatches)
        {
            entities.Add(new NamedEntity
            {
                Text = match.Value,
                Type = "EMAIL",
                StartIndex = match.Index,
                EndIndex = match.Index + match.Length
            });
        }
        
        // Detectar números de teléfono
        var phonePattern = @"\b\d{3}-\d{3}-\d{4}\b";
        var phoneMatches = Regex.Matches(text, phonePattern);
        foreach (Match match in phoneMatches)
        {
            entities.Add(new NamedEntity
            {
                Text = match.Value,
                Type = "PHONE",
                StartIndex = match.Index,
                EndIndex = match.Index + match.Length
            });
        }
        
        return entities;
    }

    private string DetectLanguage(string text)
    {
        // Detección de idioma simplificada
        var englishWords = new[] { "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };
        var spanishWords = new[] { "el", "la", "de", "que", "y", "a", "en", "un", "es", "se", "no", "te", "lo", "le" };
        
        var words = text.ToLower().Split(new char[] { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        int englishCount = words.Count(word => englishWords.Contains(word));
        int spanishCount = words.Count(word => spanishWords.Contains(word));
        
        if (englishCount > spanishCount) return "en";
        if (spanishCount > englishCount) return "es";
        return "unknown";
    }

    private string ClassifyIntent(string text)
    {
        var intents = new Dictionary<string, string[]>
        {
            ["greeting"] = new[] { "hello", "hi", "hey", "good morning", "good afternoon" },
            ["question"] = new[] { "what", "how", "when", "where", "why", "who", "?" },
            ["request"] = new[] { "can you", "could you", "please", "help", "need" },
            ["booking"] = new[] { "book", "reserve", "schedule", "appointment", "event" },
            ["complaint"] = new[] { "problem", "issue", "wrong", "bad", "terrible", "awful" },
            ["compliment"] = new[] { "great", "excellent", "amazing", "wonderful", "fantastic" }
        };
        
        var lowerText = text.ToLower();
        
        foreach (var intent in intents)
        {
            if (intent.Value.Any(keyword => lowerText.Contains(keyword)))
            {
                return intent.Key;
            }
        }
        
        return "unknown";
    }

    private IEnumerable<Entity> ExtractEntities(string text)
    {
        var entities = new List<Entity>();
        
        // Detectar fechas
        var datePattern = @"\b\d{1,2}/\d{1,2}/\d{4}\b";
        var dateMatches = Regex.Matches(text, datePattern);
        foreach (Match match in dateMatches)
        {
            entities.Add(new Entity
            {
                Text = match.Value,
                Type = "DATE",
                Confidence = 0.9f
            });
        }
        
        // Detectar precios
        var pricePattern = @"\$\d+(\.\d{2})?";
        var priceMatches = Regex.Matches(text, pricePattern);
        foreach (Match match in priceMatches)
        {
            entities.Add(new Entity
            {
                Text = match.Value,
                Type = "PRICE",
                Confidence = 0.9f
            });
        }
        
        return entities;
    }

    private SentimentResult AnalyzeSentiment(string text)
    {
        // Análisis de sentimientos simplificado
        var positiveWords = new[] { "good", "great", "excellent", "amazing", "wonderful", "fantastic", "love", "like" };
        var negativeWords = new[] { "bad", "terrible", "awful", "horrible", "disappointing", "poor", "hate", "dislike" };
        
        var words = text.ToLower().Split(new char[] { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        int positiveCount = words.Count(word => positiveWords.Contains(word));
        int negativeCount = words.Count(word => negativeWords.Contains(word));
        
        if (positiveCount + negativeCount == 0)
        {
            return new SentimentResult
            {
                Sentiment = "Neutral",
                Confidence = 0.5f
            };
        }
        
        var confidence = Math.Abs(positiveCount - negativeCount) / (float)(positiveCount + negativeCount);
        var sentiment = positiveCount > negativeCount ? "Positive" : "Negative";
        
        return new SentimentResult
        {
            Sentiment = sentiment,
            Confidence = confidence
        };
    }
}
```

### **2. Sistema de Chatbot Inteligente**

#### **Chatbot para MussikOn**
```csharp
public class MussikOnChatbot : IChatbotService
{
    private readonly IAdvancedTextProcessor _textProcessor;
    private readonly IMusicianRepository _musicianRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<MussikOnChatbot> _logger;
    
    private readonly Dictionary<string, Func<ChatContext, Task<string>>> _intentHandlers;

    public MussikOnChatbot(
        IAdvancedTextProcessor textProcessor,
        IMusicianRepository musicianRepository,
        IEventRepository eventRepository,
        ILogger<MussikOnChatbot> logger)
    {
        _textProcessor = textProcessor;
        _musicianRepository = musicianRepository;
        _eventRepository = eventRepository;
        _logger = logger;
        
        _intentHandlers = InitializeIntentHandlers();
    }

    public async Task<ChatResponse> ProcessMessageAsync(ChatRequest request)
    {
        try
        {
            var context = new ChatContext
            {
                UserId = request.UserId,
                SessionId = request.SessionId,
                Message = request.Message,
                Timestamp = DateTime.UtcNow
            };

            // Procesar texto
            var processedText = await _textProcessor.ProcessTextAsync(request.Message);
            context.ProcessedText = processedText;

            // Determinar intención
            var intent = processedText.Intent;
            context.Intent = intent;

            // Manejar intención
            string response;
            if (_intentHandlers.ContainsKey(intent))
            {
                response = await _intentHandlers[intent](context);
            }
            else
            {
                response = await HandleUnknownIntent(context);
            }

            return new ChatResponse
            {
                Message = response,
                Intent = intent,
                Confidence = processedText.Sentiment.Confidence,
                Suggestions = GenerateSuggestions(intent),
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return new ChatResponse
            {
                Message = "I'm sorry, I encountered an error. Please try again.",
                Intent = "error",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private Dictionary<string, Func<ChatContext, Task<string>>> InitializeIntentHandlers()
    {
        return new Dictionary<string, Func<ChatContext, Task<string>>>
        {
            ["greeting"] = HandleGreeting,
            ["question"] = HandleQuestion,
            ["request"] = HandleRequest,
            ["booking"] = HandleBooking,
            ["complaint"] = HandleComplaint,
            ["compliment"] = HandleCompliment
        };
    }

    private async Task<string> HandleGreeting(ChatContext context)
    {
        var greetings = new[]
        {
            "Hello! Welcome to MussikOn. How can I help you today?",
            "Hi there! I'm here to help you find the perfect musician for your event.",
            "Welcome! What kind of musical talent are you looking for?"
        };
        
        return greetings[new Random().Next(greetings.Length)];
    }

    private async Task<string> HandleQuestion(ChatContext context)
    {
        var message = context.Message.ToLower();
        
        if (message.Contains("how") && message.Contains("work"))
        {
            return "MussikOn connects event organizers with talented musicians. You can browse profiles, listen to samples, and book musicians directly through our platform.";
        }
        
        if (message.Contains("price") || message.Contains("cost"))
        {
            return "Pricing varies by musician and event type. You can see rates on each musician's profile, and they typically range from $100-$500 per hour depending on experience and location.";
        }
        
        if (message.Contains("available") || message.Contains("schedule"))
        {
            return "Each musician's availability is shown on their profile. You can filter by date and time to find musicians available for your event.";
        }
        
        return "I'd be happy to help with your question. Could you provide more details about what you'd like to know?";
    }

    private async Task<string> HandleRequest(ChatContext context)
    {
        var message = context.Message.ToLower();
        
        if (message.Contains("find") || message.Contains("search"))
        {
            return "I can help you find musicians! What type of music are you looking for? (e.g., jazz, rock, classical, acoustic)";
        }
        
        if (message.Contains("recommend"))
        {
            return "I'd be happy to recommend musicians! What's the style and size of your event?";
        }
        
        if (message.Contains("help"))
        {
            return "I'm here to help! I can assist with finding musicians, answering questions about our platform, or helping with bookings. What do you need?";
        }
        
        return "I'm here to help! Could you tell me more about what you need?";
    }

    private async Task<string> HandleBooking(ChatContext context)
    {
        var message = context.Message.ToLower();
        
        if (message.Contains("book") || message.Contains("reserve"))
        {
            return "Great! To book a musician, you'll need to: 1) Find a musician you like, 2) Check their availability, 3) Send a booking request. Do you have a specific musician in mind?";
        }
        
        if (message.Contains("event"))
        {
            return "Tell me about your event! What type of event is it, and what kind of music are you looking for?";
        }
        
        return "I can help you with booking! What type of event are you planning?";
    }

    private async Task<string> HandleComplaint(ChatContext context)
    {
        return "I'm sorry to hear you're having an issue. Let me help you resolve this. Can you tell me more about the problem you're experiencing?";
    }

    private async Task<string> HandleCompliment(ChatContext context)
    {
        var responses = new[]
        {
            "Thank you so much! I'm glad I could help.",
            "That's very kind of you! I'm here whenever you need assistance.",
            "I appreciate your feedback! Is there anything else I can help you with?"
        };
        
        return responses[new Random().Next(responses.Length)];
    }

    private async Task<string> HandleUnknownIntent(ChatContext context)
    {
        return "I'm not sure I understand. Could you rephrase your question? I can help you find musicians, answer questions about our platform, or assist with bookings.";
    }

    private IEnumerable<string> GenerateSuggestions(string intent)
    {
        var suggestions = new Dictionary<string, string[]>
        {
            ["greeting"] = new[] { "Find musicians", "How does it work?", "View events" },
            ["question"] = new[] { "Search musicians", "Browse categories", "Contact support" },
            ["request"] = new[] { "Find jazz musicians", "Find acoustic guitarists", "Find wedding bands" },
            ["booking"] = new[] { "Browse available musicians", "Check pricing", "Create event" },
            ["complaint"] = new[] { "Contact support", "Report issue", "Get help" },
            ["compliment"] = new[] { "Find musicians", "Browse events", "Share feedback" }
        };
        
        return suggestions.ContainsKey(intent) ? suggestions[intent] : new[] { "Find musicians", "Get help", "Browse events" };
    }
}
```

### **3. Análisis de Conversaciones**

#### **Sistema de Análisis de Diálogos**
```csharp
public class ConversationAnalysisService : IConversationAnalysisService
{
    private readonly IAdvancedTextProcessor _textProcessor;
    private readonly ILogger<ConversationAnalysisService> _logger;

    public ConversationAnalysisService(
        IAdvancedTextProcessor textProcessor,
        ILogger<ConversationAnalysisService> logger)
    {
        _textProcessor = textProcessor;
        _logger = logger;
    }

    public async Task<ConversationAnalysisResult> AnalyzeConversationAsync(IEnumerable<ChatMessage> messages)
    {
        try
        {
            var analysis = new ConversationAnalysisResult
            {
                TotalMessages = messages.Count(),
                Participants = messages.Select(m => m.UserId).Distinct().Count(),
                Duration = CalculateDuration(messages),
                SentimentTrend = AnalyzeSentimentTrend(messages),
                Topics = ExtractTopics(messages),
                IntentDistribution = AnalyzeIntentDistribution(messages),
                ResponseTime = CalculateResponseTime(messages),
                EngagementScore = CalculateEngagementScore(messages)
            };

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing conversation");
            throw;
        }
    }

    private TimeSpan CalculateDuration(IEnumerable<ChatMessage> messages)
    {
        if (!messages.Any()) return TimeSpan.Zero;
        
        var firstMessage = messages.Min(m => m.Timestamp);
        var lastMessage = messages.Max(m => m.Timestamp);
        
        return lastMessage - firstMessage;
    }

    private IEnumerable<SentimentTrendPoint> AnalyzeSentimentTrend(IEnumerable<ChatMessage> messages)
    {
        var trendPoints = new List<SentimentTrendPoint>();
        
        foreach (var message in messages.OrderBy(m => m.Timestamp))
        {
            var processedText = _textProcessor.ProcessTextAsync(message.Content).Result;
            
            trendPoints.Add(new SentimentTrendPoint
            {
                Timestamp = message.Timestamp,
                Sentiment = processedText.Sentiment.Sentiment,
                Confidence = processedText.Sentiment.Confidence
            });
        }
        
        return trendPoints;
    }

    private IEnumerable<string> ExtractTopics(IEnumerable<ChatMessage> messages)
    {
        var allText = string.Join(" ", messages.Select(m => m.Content));
        var processedText = _textProcessor.ProcessTextAsync(allText).Result;
        
        return processedText.Entities
            .Where(e => e.Type == "TOPIC")
            .Select(e => e.Text)
            .Distinct()
            .Take(10);
    }

    private Dictionary<string, int> AnalyzeIntentDistribution(IEnumerable<ChatMessage> messages)
    {
        var intentCounts = new Dictionary<string, int>();
        
        foreach (var message in messages)
        {
            var processedText = _textProcessor.ProcessTextAsync(message.Content).Result;
            var intent = processedText.Intent;
            
            if (intentCounts.ContainsKey(intent))
            {
                intentCounts[intent]++;
            }
            else
            {
                intentCounts[intent] = 1;
            }
        }
        
        return intentCounts;
    }

    private TimeSpan CalculateResponseTime(IEnumerable<ChatMessage> messages)
    {
        var responseTimes = new List<TimeSpan>();
        var orderedMessages = messages.OrderBy(m => m.Timestamp).ToList();
        
        for (int i = 1; i < orderedMessages.Count; i++)
        {
            var currentMessage = orderedMessages[i];
            var previousMessage = orderedMessages[i - 1];
            
            if (currentMessage.UserId != previousMessage.UserId)
            {
                responseTimes.Add(currentMessage.Timestamp - previousMessage.Timestamp);
            }
        }
        
        return responseTimes.Any() ? 
            TimeSpan.FromTicks((long)responseTimes.Average(rt => rt.Ticks)) : 
            TimeSpan.Zero;
    }

    private float CalculateEngagementScore(IEnumerable<ChatMessage> messages)
    {
        if (!messages.Any()) return 0;
        
        var messageCount = messages.Count();
        var participantCount = messages.Select(m => m.UserId).Distinct().Count();
        var duration = CalculateDuration(messages);
        
        // Fórmula simple de engagement
        var messagesPerMinute = messageCount / Math.Max(1, duration.TotalMinutes);
        var participationRate = participantCount / (float)messageCount;
        
        return Math.Min(100, (float)(messagesPerMinute * 10 + participationRate * 50));
    }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Chatbot Básico**
```csharp
// Implementar chatbot simple
public class BasicChatbot
{
    private readonly Dictionary<string, string> _responses;

    public BasicChatbot()
    {
        _responses = new Dictionary<string, string>
        {
            ["hello"] = "Hi there! How can I help you?",
            ["help"] = "I can help you with questions about our service.",
            ["goodbye"] = "Goodbye! Have a great day!",
            ["thanks"] = "You're welcome! Is there anything else I can help with?"
        };
    }

    public string ProcessMessage(string message)
    {
        var lowerMessage = message.ToLower();
        
        foreach (var response in _responses)
        {
            if (lowerMessage.Contains(response.Key))
            {
                return response.Value;
            }
        }
        
        return "I'm not sure I understand. Could you rephrase that?";
    }
}
```

### **Ejercicio 2: API de Chatbot**
```csharp
// Crear API REST para chatbot
[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;
    private readonly IConversationAnalysisService _analysisService;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(
        IChatbotService chatbotService,
        IConversationAnalysisService analysisService,
        ILogger<ChatbotController> logger)
    {
        _chatbotService = chatbotService;
        _analysisService = analysisService;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var response = await _chatbotService.ProcessMessageAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<ConversationAnalysisResult>> AnalyzeConversation(
        [FromBody] IEnumerable<ChatMessage> messages)
    {
        try
        {
            var result = await _analysisService.AnalyzeConversationAsync(messages);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing conversation");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Procesamiento de Lenguaje Natural**: Análisis avanzado de texto
2. **Chatbots Inteligentes**: Sistemas conversacionales
3. **Análisis de Conversaciones**: Métricas y insights
4. **Clasificación de Intenciones**: Reconocimiento de propósito
5. **Extracción de Entidades**: Identificación de información clave

### **Habilidades Desarrolladas:**
- Implementación de NLP con .NET
- Creación de chatbots conversacionales
- Análisis de diálogos y conversaciones
- APIs REST para chatbots
- Sistemas de procesamiento de texto

### **Próxima Clase:**
**Clase 6: Predictive Analytics y Forecasting**
- Análisis predictivo con ML.NET
- Modelos de forecasting
- Predicción de tendencias
- Análisis de series temporales

---

## 🔗 **Enlaces Útiles**
- [ML.NET Text Processing](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/prepare-data-ml-net)
- [Natural Language Processing](https://docs.microsoft.com/en-us/azure/cognitive-services/language-service/)
- [Chatbot Development](https://docs.microsoft.com/en-us/azure/bot-service/)
- [NLP with .NET](https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/sentiment-analysis)
- [Conversational AI](https://docs.microsoft.com/en-us/azure/cognitive-services/language-service/conversational-language-understanding/overview)
