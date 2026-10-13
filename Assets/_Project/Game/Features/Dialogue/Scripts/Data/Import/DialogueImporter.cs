using System.Collections.Generic;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueImporter
    {
        private readonly DialogueJsonReader reader;
        private readonly DialogueValidator validator;
        private readonly DialogueBuilder builder;

        public DialogueImporter(
            DialogueJsonReader reader,
            DialogueValidator validator,
            DialogueBuilder builder)
        {
            this.reader = reader;
            this.validator = validator;
            this.builder = builder;
        }

        public List<Dialogue> Import(
            string jsonPath)
        {
            DialogueJsonData data =
                reader.Read(jsonPath);

            if (data == null)
            {
                return null;
            }

            if (!validator.Validate(data))
            {
                return null;
            }

            return builder.Build(data);
        }
    }
}
