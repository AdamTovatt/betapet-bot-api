using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication.Responses
{
    public enum CodeType
    {
        Unknown, OccupiedSquare, Word, InvalidTiles
    }

    public class PlayMoveResponse : BetapetResponse
    {
        [JsonProperty("resut")]
        public bool Result { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        public CodeType CodeType {get; private set; }

        public static PlayMoveResponse FromJson(string json)
        {
            PlayMoveResponse moveResponse = JsonConvert.DeserializeObject<PlayMoveResponse>(json);
            moveResponse.CodeType = GetCodeType(moveResponse.Code);

            if (moveResponse.CodeType == CodeType.Word)
                moveResponse.Result = true;

            return moveResponse;
        }

        private static CodeType GetCodeType(string code)
        {
            if (code == "occupied_square")
                return CodeType.OccupiedSquare;
            if (code == "word")
                return CodeType.Word;
            if (code == "bogus_tiles")
                return CodeType.InvalidTiles;

            return CodeType.Unknown;
        }
    }
}
