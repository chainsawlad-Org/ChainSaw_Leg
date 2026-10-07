using Cysharp.Threading.Tasks;

public interface IApplicationService
{
    UniTask Initialize();
    UniTask Dispose();
}
