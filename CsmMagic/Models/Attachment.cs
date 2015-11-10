using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trebuchet;
using Trebuchet.API;

namespace CsmMagic.Models
{
    public class Attachment
    {
        // Output Stuff goes under here
        public Stream OutputAttachment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ShortCutText { get; set; }
        public string FileExtension { get; set; }
        public string RecId { get; set; }
        public string Error { get; set; }
       

        // Input Stuff goes under here
        public FileInfo InputAttachment { get; set; }
       
        // Parameter-less constructor for sakes
        public Attachment()
        {
        }

        // Output Constructor
        internal Attachment(ShortcutInfo shortcut, Stream attachment)
        {
            CreatedAt = shortcut.Created;
            ShortCutText = shortcut.ShortcutText;
            FileExtension = shortcut.AttachmentFileType;
            OutputAttachment = attachment;
            RecId = shortcut.ShortcutTargetId;
        }

        // Input Constructor
        internal Attachment(FileInfo attachment)
        {
            InputAttachment = attachment;
        }

        internal ShortcutInfo GetShortcutInfoFromFileInfo()
        {
            var shortcut = ShortcutInfo.Create(ShortcutType.File);
            shortcut.AttachmentType = AttachmentTypes.Imported;
            shortcut.AttachmentFileName = InputAttachment.FullName;
            shortcut.AttachmentFileType = InputAttachment.Extension;
            shortcut.ShortcutText = InputAttachment.Name;
            return shortcut;
        }
    }
}
