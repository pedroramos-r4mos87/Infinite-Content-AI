using InfiniteContentAI.SharedKernel.Results;

namespace InfiniteContentAI.Application.ArtificialIntelligence;

public static class AIProviderErrors
{
    public static readonly Error ResearchFailed = Error.Failure(
        "AI.ResearchFailed",
        "Não foi possível gerar a pesquisa.");

    public static readonly Error ScriptFailed = Error.Failure(
        "AI.ScriptFailed",
        "Não foi possível gerar o roteiro.");

    public static readonly Error UnexpectedFailure = Error.Failure(
        "AI.UnexpectedFailure",
        "O provider de inteligência artificial falhou inesperadamente.");
}
