using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Toolkit.Services;

namespace Toolkit.Tests
{
    public class EventsHandlerTests
    {
        private class EventsHandlerMock : EventsHandler<Action>
        {
            private StringBuilder m_Log;

            public EventsHandlerMock(StringBuilder log)
            {
                m_Log = log;
            }

            protected override void SubscribeEvents()
            {
                m_Log.Append("s");
            }

            protected override void UnsubscribeEvents()
            {
                m_Log.Append("u");
            }

            public void RaiseEvent() 
            {
                Delegate?.Invoke();
            }
        }

        private class ObserverMock 
        {
            public EventsHandlerMock Handler { get; }

            public event Action Event 
            {
                add 
                {
                    Handler.Attach(value);
                }
                remove
                {
                    Handler.Detach(value);
                }
            }

            public ObserverMock(StringBuilder log) 
            {
                Handler = new EventsHandlerMock(log);
            }
        }

        [Test]
        public void SubscribeUnsubscribeTest() 
        {
            var l = new StringBuilder();

            var obs = new ObserverMock(l);

            var res = "";

            var del = new Action(() => 
            {
                res += "A";
            });

            obs.Event += del;
            var l1 = l.ToString();
            obs.Event += del;
            var l2 = l.ToString();

            obs.Handler.RaiseEvent();

            Action del1 = null;
            Action del2 = null;

            obs.Event -= del;
            var l3 = l.ToString();
            del1 = obs.Handler.Delegate;
            obs.Event -= del;
            var l4 = l.ToString();
            del2 = obs.Handler.Delegate;

            Assert.AreEqual("AA", res);
            Assert.AreEqual("s", l1);
            Assert.AreEqual("s", l2);
            Assert.AreEqual("s", l3);
            Assert.AreEqual("su", l4);
            Assert.IsNotNull(del1);
            Assert.IsNull(del2);
        }
    }
}
