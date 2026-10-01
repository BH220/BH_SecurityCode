using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model.Response
{
    public class ResFile
    {
        /// <summary>
        /// 서버 자체가 죽어버렸는지 여부를 반환
        /// 1: 서버가 살아 있음
        /// 0: 서버가 죽어 있거나 찾지 못함
        /// </summary>
        public int is_alive { get; set; } = 0;
        /// <summary>
        /// API 요청이 성공적으로 처리되었는지 반환
        /// 1: API 요청이 성공적으로 처리됨
        /// 0: API 요청이 실패했거나 처리 중 오류 발생
        /// </summary>
        public int result { get; set; } = 0;
        public string msg { get; set; } = "";
        public string file_name { get; set; } = "";
        public string file_ext
        {
            get
            {
                return Path.GetExtension(file_name);
            }
        }
        public string file_name_without_ext
        {
            get
            {
                return Path.GetFileNameWithoutExtension(file_name);
            }
        }
        public long file_size { get; set; } = 0;
        public string file_directory { get; set; } = "";
        public string pull_path
        {
            get
            {
                return $"{file_directory}\\{file_name}";
            }
        }
    }
}
