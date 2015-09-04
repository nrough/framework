using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using Infovision.Utils;

namespace Infovision.MRI.DAL
{
    [Serializable]
    public class MiningObject : IMiningObject
    {
        event EventHandler<MiningObjectEventArgs> PreExecuteEvent;
        event EventHandler<MiningObjectEventArgs> PostExecuteEvent;
        event EventHandler<MiningObjectEventArgs> PreViewEvent;
        event EventHandler<MiningObjectEventArgs> PostViewEvent;

        private object objectLock = new Object();

        private Args parameters = new Args();

        public MiningObject()
        {
        }

        public static IMiningObject Create(string typeId)
        {
            IMiningObject ret;

            switch (typeId)
            {
                case MiningObjectType.Types.ImageRAW :
                case MiningObjectType.Types.ImageITK :
                    ret = new MiningObjectImage();
                    break;

                case MiningObjectType.Types.ImageExtract :
                    ret = new MiningObjectImageExtract();
                    break;

                case MiningObjectType.Types.ImageHistogram :
                    ret = new MiningObjectImageHistogram();
                    break;

                case MiningObjectType.Types.ImageMask :
                    ret = new MiningObjectImageMask();
                    break;

                case MiningObjectType.Types.ImageEdge :
                    ret = new MiningObjectImageEdge();
                    break;

                case MiningObjectType.Types.ImageSOMCluster :
                    ret = new MiningObjectSOMCluster();
                    break;

                case MiningObjectType.Types.ImageHistogramCluster :
                    ret = new MiningObjectHistogramCluster();
                    break;

                case MiningObjectType.Types.ImageNeighbour :
                    ret = new MiningObjectNeighbour();
                    break;

                default:
                    ret = new MiningObject();
                    break;
            }

            ret.TypeId = typeId;

            return ret;
        }

        public static IMiningObject Create(MiningObjectViewModel viewModel)
        {
            IMiningObject ret = (IMiningObject)Activator.CreateInstance(viewModel.GetMiningObjectType());
            ret.InitFromViewModel(viewModel);
            return ret;
        }
        
        public string Name { get; set; }
        public long Id { get; set; }
        public string TypeId { get; set; }
        public long RefId { get; set; }

        public void AddParameter(string name, object value)
        {
            parameters.AddParameter(name, value);
        }

        public object GetParameter(string name)
        {
            return parameters.GetParameter(name);
        }

        public virtual XElement XMLParametersElement 
        {
            get { return new XElement("Parameters", null); }
        }

        public virtual void XMLParseParameters(XElement parametersElement)
        {
        }

        public virtual void InitFromViewModel(MiningObjectViewModel propertyModel)
        {
        }

        protected string XMLGetParameterValue(XElement parametersElement, string name)
        {
            var value = from p in parametersElement.Descendants("Parameter")
                        where p.Attribute("Name").Value == name
                        select p.Attribute("Value").Value;

            return value.Single<string>();
        }

        public virtual void ReloadReferences(MiningProject project)
        {
        }
        
        event EventHandler<MiningObjectEventArgs> IMiningObject.Executing
        {
            add
            {
                lock (objectLock)
                {
                    PreExecuteEvent += value;
                }

            }
            remove
            {
                lock (objectLock)
                {
                    PreExecuteEvent -= value;
                }
            }
        }

        event EventHandler<MiningObjectEventArgs> IMiningObject.Executed
        {
            add
            {
                lock (objectLock)
                {
                    PostExecuteEvent += value;
                }
            }
            remove
            {
                lock (objectLock)
                {
                    PostExecuteEvent -= value;
                }
            }
        }

        event EventHandler<MiningObjectEventArgs> IMiningObject.Viewing
        {
            add
            {
                lock (objectLock)
                {
                    PreViewEvent += value;
                }

            }
            remove
            {
                lock (objectLock)
                {
                    PreViewEvent -= value;
                }
            }
        }

        event EventHandler<MiningObjectEventArgs> IMiningObject.Viewed
        {
            add
            {
                lock (objectLock)
                {
                    PostViewEvent += value;
                }
            }
            remove
            {
                lock (objectLock)
                {
                    PostViewEvent -= value;
                }
            }
        }

        public void Execute()
        {
            MiningObjectEventArgs e = new MiningObjectEventArgs(this);

            this.OnExecuting(e);
            this.OnExecuted(e);
        }
        
        public void View()
        {
            MiningObjectEventArgs e = new MiningObjectEventArgs(this);

            this.OnViewing(e);
            this.OnViewed(e);
        }

        private void OnExecuting(MiningObjectEventArgs e)
        {
            EventHandler<MiningObjectEventArgs> handler = PreExecuteEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnExecuted(MiningObjectEventArgs e)
        {
            EventHandler<MiningObjectEventArgs> handler = PostExecuteEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnViewing(MiningObjectEventArgs e)
        {
            EventHandler<MiningObjectEventArgs> handler = PreViewEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnViewed(MiningObjectEventArgs e)
        {
            EventHandler<MiningObjectEventArgs> handler = PostViewEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
