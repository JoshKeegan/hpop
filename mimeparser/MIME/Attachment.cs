/*
*Name:			OpenPOP.MIMEParser.Attachment
*Function:		
*Author:		Hamid Qureshi
*Created:		2003/8
*Modified:		2004/3/29 17:32 GMT+8
*Description	:
*Changes:		2004/3/29 10:28 GMT+8 by Unruled Boy
*					1.removing bugs in decoding attachment
*Changes:		2004/3/29 17:32 GMT+8 by Unruled Boy
*					1.support for reply message using ms-tnef 
*					2.adding detail description for every function
*					3.cleaning up the codes
*/

using System;
using System.IO;
using System.Text;

namespace OpenPOP.MIMEParser
{
	/// <summary>
	/// Summary description for Attachment.
	/// </summary>
	public class Attachment
	{
		#region Member Variables
		private string _contentType=null;
		private string _contentCharset=null;
		private string _contentFormat=null;
		private string _contentTransferEncoding=null;
		private string _contentDescription=null;
		private string _contentDisposition=null;
		private string _contentFileName="";
		private string _defaultFileName="body.htm";
		private string _defaultFileName2="body*.htm";
		private string _defaultReportFileName="report.htm";
		private string _defaultMIMEFileName="body.eml";
		private string _defaultMSTNEFFileName="winmail.dat";
		private string _contentID=null;
		private long _contentLength=0;
		private string _rawAttachment=null;
		private bool _inBytes=false;
		private byte[] _rawBytes=null;
		#endregion

		#region Properties
		public byte[] RawBytes
		{
			get{return _rawBytes;}
			set{_rawBytes=value;}
		}

		public bool InBytes
		{
			get{return _inBytes;}
			set{_inBytes=value;}
		}

		public long ContentLength
		{
			get{return _contentLength;}
		}

		/// <summary>
		/// verify the attachment whether it is a real attachment or not
		/// </summary>
		public bool NotAttachment
		{
			get
			{
/*				if (_contentDisposition==null||_contentType==null)
					return true;
				else
					return (_contentDisposition.IndexOf("attachment")==-1 && _contentType.IndexOf("text/plain")!=-1); */
/*				if (_contentType==null)
					return true;
				else
					return (_contentFileName!="");*/
				if (_contentType==null||_contentFileName=="")
					return true;
				else
					return false;

			}
		}

		public string ContentFormat
		{
			get{return _contentFormat;}
		}

		public string ContentCharset
		{
			get{return _contentCharset;}
		}

		public string DefaultFileName
		{
			get{return _defaultFileName;}
		}

		public string DefaultFileName2
		{
			get{return _defaultFileName2;}
		}

		public string DefaultReportFileName
		{
			get{return _defaultReportFileName;}
		}

		public string DefaultMIMEFileName
		{
			get{return _defaultMIMEFileName;}
		}

		public string ContentType
		{
			get{return _contentType;}
		}

		public string ContentTransferEncoding
		{
			get{return _contentTransferEncoding;}
		}

		public string ContentDescription
		{
			get{return _contentDescription;}
		}

		public string ContentFileName
		{
			get{return _contentFileName;}
			set{_contentFileName=value;}
		}

		public string ContentDisposition
		{
			get{return _contentDisposition;}
		}

		public string ContentID
		{
			get{return _contentID;}
		}

		public string RawAttachment
		{
			get{return _rawAttachment;}
		}

		/// <summary>
		/// decoded attachment in bytes
		/// </summary>
		public byte[] DecodedAttachment
		{
			get
			{
				return DecodedAsBytes();
			}
		}
		#endregion

		public Attachment(byte[] bytAttachment, long lngFileLength, string strFileName, string strContentType)
		{
			_inBytes=true;
			_rawBytes=bytAttachment;
			_contentLength=lngFileLength;
			_contentFileName=strFileName;
			_contentType=strContentType;
		}

		public Attachment(string strAttachment,string strContentType, bool blnParseHeader)
		{
			if(!blnParseHeader)
			{
				_contentFileName=_defaultMSTNEFFileName;
				_contentType=strContentType;
			}
			this.NewAttachment(strAttachment,blnParseHeader);
		}

		public Attachment(string strAttachment)
		{	
			this.NewAttachment(strAttachment,true);
		}

		/// <summary>
		/// create attachment
		/// </summary>
		/// <param name="strAttachment">raw attachment text</param>
		/// <param name="blnParseHeader">parse header</param>
		private void NewAttachment(string strAttachment, bool blnParseHeader)
		{
			_inBytes=false;

			if(strAttachment==null)
				throw new ArgumentNullException("strAttachment");

			StringReader srReader=new StringReader(strAttachment);

			if(blnParseHeader)
			{
				string strLine=srReader.ReadLine();
				while(Utility.IsNotNullTextEx(strLine))
				{
					parseHeader(srReader,ref strLine);
					if(Utility.IsOrNullTextEx(strLine))
						break;
					else
						strLine=srReader.ReadLine();
				}
			}

			this._rawAttachment=srReader.ReadToEnd();
			_contentLength=this._rawAttachment.Length;
		}

		/// <summary>
		/// Parse header fields and set member variables
		/// </summary>
		/// <param name="srReader">string reader</param>
		/// <param name="strLine">header line</param>
		private void parseHeader(StringReader srReader,ref string strLine)
		{
			string []array=Utility.getHeadersValue(strLine);
			string []values=array[1].Split(';');
			string strRet=null;

			switch(array[0].ToUpper())
			{
				case "CONTENT-TYPE":
					if(values.Length>0)
						_contentType=values[0].Trim();
					if(values.Length>1)
					{
						_contentCharset=Utility.GetQuotedValue(values[1],"=","charset");
					}
					if(values.Length>2)
					{
						_contentFormat=Utility.GetQuotedValue(values[2],"=","format");
					}
					if(_contentType.ToLower().IndexOf("name".ToLower())==-1)
					{
						strRet=srReader.ReadLine();
						if(strRet.IndexOf("filename=\"")!=-1)
						{
							_contentFileName=Utility.GetQuotedValue(strRet,"=","filename");
							_contentFileName=Utility.deCode(_contentFileName);
						}
						else if(strRet.IndexOf("name=\"")!=-1)
						{
							_contentFileName=Utility.GetQuotedValue(strRet,"=","name");
							_contentFileName=Utility.deCode(_contentFileName);
						}
						else if(strRet=="")
						{
							strLine="";
							break;
						}
						else
						{
							parseHeader(srReader,ref strRet);
						}
					}
					break;
				case "CONTENT-TRANSFER-ENCODING":
					_contentTransferEncoding=Utility.splitOnSemiColon(array[1])[0].Trim();
					break;
				case "CONTENT-DESCRIPTION":
					_contentDescription=Utility.deCode(Utility.splitOnSemiColon(array[1])[0].Trim());
					break;
				case "CONTENT-DISPOSITION":
					if(values.Length>0)
						_contentDisposition=values[0].Trim();
					
					_contentFileName=values[1];
					
					if(_contentFileName=="")
						_contentFileName=srReader.ReadLine();

					_contentFileName=Utility.GetQuotedValue(_contentFileName,"=","filename");
					_contentFileName=Utility.deCode(_contentFileName);
					break;
				case "CONTENT-ID":
					_contentID=Utility.splitOnSemiColon(array[1])[0].Trim('<').Trim('>');
					break;
			}
		}

		/// <summary>
		/// verify the encoding
		/// </summary>
		/// <param name="encoding">encoding to verify</param>
		/// <returns>true if encoding</returns>
		private bool IsEncoding(string encoding)
		{
			return _contentTransferEncoding.ToLower().IndexOf(encoding.ToLower())!=-1;
		}

		/// <summary>
		/// Decode the attachment to text
		/// </summary>
		/// <returns>Decoded attachment text</returns>
		public string DecodeAsText()
		{
			string decodedAttachment=null;

			if(_contentType.ToLower()=="message/rfc822".ToLower())
				decodedAttachment=Utility.deCode(_rawAttachment);
			else if(_contentTransferEncoding!=null)
			{
				decodedAttachment=_rawAttachment;

				if(!IsEncoding("7bit"))
				{
					if(IsEncoding("8bit")&&_contentCharset!=null&_contentCharset!="")
						decodedAttachment=Utility.Change(decodedAttachment,_contentCharset);

					if(Utility.IsQuotedPrintable(_contentTransferEncoding))
						decodedAttachment=DecodeQP.ConvertHexContent(decodedAttachment);
					else if(IsEncoding("8bit"))
						decodedAttachment=decodedAttachment;
					else
						decodedAttachment=Utility.deCodeB64s(Utility.RemoveNonB64(decodedAttachment));
				}
			}
			else if(_contentCharset!=null)
				decodedAttachment=Utility.Change(_rawAttachment,_contentCharset);//Encoding.Default.GetString(Encoding.GetEncoding(_contentCharset).GetBytes(_rawAttachment));
			else
				decodedAttachment=_rawAttachment;

			return decodedAttachment;
		}

		/// <summary>
		/// decode attachment to be a message object
		/// </summary>
		/// <returns>message</returns>
		public Message DecodeAsMessage()
		{
			bool blnRet=false;
			return new Message(ref blnRet,"",false ,_rawAttachment,false);
		}

		/// <summary>
		/// Decode the attachment to bytes
		/// </summary>
		/// <returns>Decoded attachment bytes</returns>
		public byte[] DecodedAsBytes()
		{
			if(_rawAttachment==null)
				return null;
			if(_contentFileName!="")
			{
				byte []decodedBytes=null;

				if(_contentType!=null && _contentType.ToLower()=="message/rfc822".ToLower())
					decodedBytes=Encoding.Default.GetBytes(Utility.deCode(_rawAttachment));
				else if(_contentTransferEncoding!=null)
				{
					string content=_rawAttachment;

					if(!IsEncoding("7bit"))
					{
						if(IsEncoding("8bit")&&_contentCharset!=null&_contentCharset!="")
							content=Utility.Change(content,_contentCharset);

						if(Utility.IsQuotedPrintable(_contentTransferEncoding))
							decodedBytes=Encoding.Default.GetBytes(DecodeQP.ConvertHexContent(content));
						else if(IsEncoding("8bit"))
							decodedBytes=Encoding.Default.GetBytes(content);
						else
							decodedBytes=Convert.FromBase64String(Utility.RemoveNonB64(content));
					}
					else
						decodedBytes=Encoding.Default.GetBytes(content);
				}
				else if(_contentCharset!=null)
					decodedBytes=Encoding.Default.GetBytes(Utility.Change(_rawAttachment,_contentCharset));//Encoding.Default.GetString(Encoding.GetEncoding(_contentCharset).GetBytes(_rawAttachment));
				else
					decodedBytes=Encoding.Default.GetBytes(_rawAttachment);

				return decodedBytes;
			}
			else
			{
				return null;
			}
		}
	}
}