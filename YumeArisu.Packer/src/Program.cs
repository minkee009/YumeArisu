using YumeArisu.Core.Packaging;

namespace YumeArisu.Packer;

internal class Program
{
    public static void Main(string[] args)
    {
        if(args.Length > 0)
        {
            switch(args[0])
            {
                case "pack":
                    ExcutePackerProgram(args[1..]);
                    return;
                case "help":
                    System.Console.WriteLine("'pack' {rootFolderPath} {outputFolderPath} [outputName] : 소스파일을 지정한 경로에 압축하여 저장합니다.");
                    return;
            }
        }

        System.Console.WriteLine("'help'를 입력하여 실행할 명령어를 확인해주세요.");
    }

    private static void ExcutePackerProgram(string[] validArgs)
    {
        // 최소 2개(rootFolderPath, outputFolderPath) 이상이어야 함
        if (validArgs.Length < 2)
            throw new InvalidDataException("인자가 올바르지 않습니다");

        try
        {
            string rootFolderPath = validArgs[0];
            string outputFolderPath = validArgs[1];

            // 2번째 인자(validArgs[2])가 들어왔다면 출력 이름으로 전달, 없으면 null 전달
            string? outputName = validArgs.Length >= 3 ? validArgs[2] : null;

            PakWriter.Pack(rootFolderPath, outputFolderPath, outputName);
        }
        catch (IndexOutOfRangeException ex)
        {
            throw new Exception("인자의 개수가 적습니다.", ex);
        }
    }
}