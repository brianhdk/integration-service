using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Perfion.Infrastructure;

namespace Vertica.Integration.Perfion
{
	public class PerfionXml
	{
		private readonly IPerfionService _service;
		private readonly XDocument _document;

		public PerfionXml(IPerfionService service, XDocument document, ArchiveCreated archive)
		{
			Archive = archive;
			if (service == null) throw new ArgumentNullException("service");
			if (document == null) throw new ArgumentNullException("document");
			if (document.Root == null) throw new ArgumentException(@"Document is missing required root element.");

			_service = service;
			_document = document;
		}

		public IPerfionService Service
		{
			get { return _service; }
		}

		public XDocument Document
		{
			get { return _document; }
		}

		public ArchiveCreated Archive { get; private set; }

		public XElement Root
		{
			get { return _document.Root; }
		}

		public int Length
		{
			get { return _document.ToString().Length; }
		}

		public IEnumerable<Component> Components(XName name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return Root.Elements(name).Select(x => new Component(this, x));
		}

		public override string ToString()
		{
			return _document.ToString();
		}

		public class Image
		{
			private readonly PerfionXml _xml;
			private readonly XElement _element;

			public Image(PerfionXml xml, XElement element)
			{
				if (xml == null) throw new ArgumentNullException("xml");
				if (element == null) throw new ArgumentNullException("element");

				_element = element;
				_xml = xml;
			}

			public XElement Element
			{
				get { return _element; }
			}

			public Guid Id
			{
				get { return _element.AsGuid(); }
			}

			public string Name
			{
				get { return _element.AttributeOrThrow("string").Value; }
			}

			public DateTime LastModified
			{
				get { return _element.LastModified(); }
			}

			public DateTime FileLastModified
			{
				get { return _element.AttributeOrThrow("fileModifiedDate").AsDateTime(); }
			}

			public byte[] Download(NameValueCollection options = null)
			{
				if (options == null)
					return _xml.Service.DownloadFile(Id);

				return _xml.Service.DownloadImage(Id, options);
			}
		}

		public class Component
		{
			private readonly PerfionXml _xml;
			private readonly XElement _element;

			public Component(PerfionXml xml, XElement element)
			{
				if (xml == null) throw new ArgumentNullException("xml");
				if (element == null) throw new ArgumentNullException("element");

				_xml = xml;
				_element = element;
			}

			public XElement Element
			{
				get { return _element; }
			}

			public int Id
			{
				get { return _element.Id(); }
			}

			public int? ParentId
			{
				get { return _element.ParentId(); }
			}

			public string Name(string language = null)
			{
				return this["Value", language];
			}

			public string this[XName name, string language = null]
			{
				get
				{
					if (name == null) throw new ArgumentNullException("name");

					XElement element = _element.Element(name, language);

					return element != null ? element.Value : null;
				}
			}

			public string[] ValuesFor(XName name, string language = null)
			{
				if (name == null) throw new ArgumentNullException("name");

				return _element.Elements(name, language).Select(x => x.Value).ToArray();
			}

			public Component Parent
			{
				get
				{
					int? parentId = ParentId;

					if (!parentId.HasValue)
						return null;

					return _xml.Components(_element.Name).SingleOrDefault(x => x.Id == parentId);
				}
			}

			public DateTime LastModified
			{
				get { return _element.LastModified(); }
			}

			public int? IdOf(XName relatedComponent, string language = null)
			{
				if (relatedComponent == null) throw new ArgumentNullException("relatedComponent");

				XElement element = _element.Element(relatedComponent, language);

				if (element == null)
					return null;

				return element.Id();
			}

			public Component FindRelation(XName relatedComponent, string language = null)
			{
				if (relatedComponent == null) throw new ArgumentNullException("relatedComponent");

				int? id = IdOf(relatedComponent, language);

				if (!id.HasValue)
					return null;

				return _xml.Components(relatedComponent).FirstOrDefault(x => x.Id == id.Value);
			}

			public Image GetImage(XName name)
			{
				if (name == null) throw new ArgumentNullException("name");

				XElement element = _element.Element(name);

				if (element == null)
					return null;

				return new Image(_xml, element);
			}
		}
	}
}