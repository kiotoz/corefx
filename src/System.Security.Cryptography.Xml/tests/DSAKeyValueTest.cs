//
// DSAKeyValueTest.cs - Test Cases for DSAKeyValue
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using Xunit;

namespace System.Security.Cryptography.Xml.Tests
{

    public class DSAKeyValueTest
    {
        [Fact]
        public void Ctor_Empty()
        {
            DSAKeyValue dsaKeyValue = new DSAKeyValue();
            Assert.NotNull(dsaKeyValue.Key);
        }

        [Fact]
        public void Ctor_Dsa()
        {
            using (DSA dsa = DSA.Create())
            {
                DSAKeyValue dsaKeyValue = new DSAKeyValue(dsa);
                Assert.Equal(dsa, dsaKeyValue.Key);
            }
        }

        [Fact]
        public void Ctor_Dsa_Null()
        {
            DSAKeyValue dsaKeyValue = new DSAKeyValue(null);

            //From https://github.com/peterwurzinger:
            //This assertion is incorrect, since the parameter value is stored unvalidated/unprocessed
            //Assert.NotNull(dsaKeyValue.Key);

            Assert.Null(dsaKeyValue.Key);
        }

        [Fact]
        [ActiveIssue(17001, TestPlatforms.OSX)]
        public void GetXml()
        {
            DSAKeyValue dsa = new DSAKeyValue();
            XmlElement xmlkey = dsa.GetXml();

            XmlNamespaceManager ns = new XmlNamespaceManager(xmlkey.OwnerDocument.NameTable);
            ns.AddNamespace("schema", SignedXml.XmlDsigNamespaceUrl);

            IEnumerable<XmlNode> elements =
                new[] { "P", "Q", "G", "Y", "J", "Seed", "PgenCounter" }
                .Select(elementName => xmlkey.SelectSingleNode($"/schema:DSAKeyValue/schema:{elementName}", ns))
                .Where(element => element != null);

            //There MUST be existing elements
            Assert.NotEmpty(elements);

            //Existing elements MUST include a "Y"-Element
            Assert.True(elements.SingleOrDefault(element => element.Name == "Y") != null);

            //Existing elements MUST contain InnerText
            Assert.True(elements.All(element => !string.IsNullOrEmpty(element.InnerText)));

            //Existing elements MUST be convertible from BASE64
            elements.Select(element => Convert.FromBase64String(element.InnerText));
        }

        [Fact]
        [ActiveIssue(17001, TestPlatforms.OSX)]
        public void GetXml_SameDsa()
        {
            using (DSA dsa = DSA.Create())
            {
                DSAKeyValue dsaKeyValue1 = new DSAKeyValue(dsa);
                DSAKeyValue dsaKeyValue2 = new DSAKeyValue(dsa);
                Assert.Equal(dsaKeyValue1.GetXml(), dsaKeyValue2.GetXml());
            }
        }

        [Fact(Skip = "https://github.com/dotnet/corefx/issues/16779")]
        public void LoadXml()
        {
            const string pValue = "oDZlcdJA1Kf6UeNEIZqm4KDqA6zpX7CmEtAGWi9pgnBhWOUDVEfhswfsvTLR5BCbKfE6KoHvt5Hh8D1RcAko//iZkLZ+gds9y/5Oxape8tu3TUi1BnNPWu8ieXjMtdnpyudKFsCymssJked1rBeRePG23HTVwOV1DpopjRkjBEU=";
            const string qValue = "0JxsZhjbIteTbrtfWmt5Uif6il8=";
            const string gValue = "EOVCfv1saTWIc6Dgim24a07dqqyCJXmIT+5PrgrfV3M8/hfmaMfZtpvM0BUkXVv0dFScnN7txnSpnLWchBz0RfehL6c7Mofu/d2H1cp8zvwTasfiJhypQHDuC4p1aSXuQ1hnzzyYeHKzBH9r0PA78haL7/HnwrrscttXGhmU/L0=";
            const string yValue = "HBHSdiOJDoZhRpK+B4Ft5hisHvRjz6rELay+aPrya2yKRUUN7ZysNi12PltAvljexay0gEpPncg6TrRtH1+7usTxbgkuIwcQ3RPPIzM7y+XldbcyVUfyze5+zXy9ALiugT+zP8DOMRj9Yj6kR6ZsgbnSdlH2hGIn9NctXgRQ6Kg=";
            const string seedValue = "NKemrvYwT/4u8DNiXoPj9jO6LAg=";
            const string pgenCounterValue = "uA==";
            string dsaKey = $"<KeyValue xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><DSAKeyValue><P>{pValue}</P><Q>{qValue}</Q><G>{gValue}</G><Y>{yValue}</Y><Seed>{seedValue}</Seed><PgenCounter>{pgenCounterValue}</PgenCounter></DSAKeyValue></KeyValue>";

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(dsaKey);

            var dsaKeyValue = new DSAKeyValue();
            dsaKeyValue.LoadXml(xmlDoc.DocumentElement);

            var parameters = dsaKeyValue.Key.ExportParameters(false);
            Assert.Equal(Convert.ToBase64String(parameters.P), pValue);
            Assert.Equal(Convert.ToBase64String(parameters.Q), qValue);
            Assert.Equal(Convert.ToBase64String(parameters.G), gValue);
            Assert.Equal(Convert.ToBase64String(parameters.Y), yValue);
            Assert.NotNull(parameters.Seed);
            Assert.Equal(Convert.ToBase64String(parameters.Seed), seedValue);
            Assert.Equal(BitConverter.GetBytes(parameters.Counter)[0], Convert.FromBase64String(pgenCounterValue)[0]);
        }

        [Fact]
        public void LoadXml_Null()
        {
            DSAKeyValue dsa1 = new DSAKeyValue();
            Assert.Throws<ArgumentNullException>(() => dsa1.LoadXml(null));
        }

        [Fact]
        public void ImportDSAKeyValue()
        {
            string p = "nHoAAyz1ZrA/4ajJnY09oVYvr/juj6cJHhVKnrIpIlWRh2EHIFsDiHR7hFvTvvTwmPiNRKBqO7Q1LST5qabEkvbvhk11d5wwItTWmLqO969NkUzA2cTqebEdshDhnl8YN7DrkCdrYu0PE0ZL/ho5yp6aRYME/Zoq2qDet+He3v9ZkjlJLg5mLPK6Q6tmLeSNTob/6fABv1RCREsyvYegWp4TxLvrgb/fd6Wtt7GC4fwWQlBeRmBIYFklNcE2g6iYMQOkgM7D/a0YrhUI3Vy3vfFNR/kKF+OBhB82V+QPhqPbdkXA+StpwOYj48mHHyr3ZvmlQdHJSkEY/p5JFbG8vw==";
            string q = "ni1fE84dSrJerbAmEAYI/0w+q97KSFmqrzUFyHk2V48=";
            string g = "m46FXiDGgIUpO7ehefrbdGqVn1+VOEc6Cyxi5xKXIDTYtLST6wy+FPybWOuYv1Sya8AeEOmz6HAht/k3GQZwVkt1ZSZCJZw8/p/zwyHTST+NdnCHXGDGvFh/wnEa0UsmTBmqBsxPiCOahWXsQ4VvTI5exRsu+/qwc/Nqbw1K8eq6pzn67ZA2/RyLepLPZGBo9vkdPmYtFKRj3PmC7/mjVWfA3tyPa0coFIx9XtjPiVtnmG9lJ9dVhGIY2cO7j8iDdBLdx6enMu67sDyHlPBDpLYSRx2Qqkcy1IdlAyxF6fOnxsDBC/N7LXNHea52XQn1TO2CvbIG7KXI8SxLr+CVbA==";
            string y = "Yt23Dtp7UT1h/HsE6qlLBX+PIwdK0N0p0wFdWmIkw91hD41Y7OKC5xXs5jkHqSfQtiGYsUaS4aYoxQiuE9wnjiPUEii5Zau7s3jVkeZAvmRDQkjvl6SPb7kNIcw2PYdL9wSsL1euaTCWeH6m7fhO0tBJN0INYFXZ3ca4bWBYwNgs2UgCooaoO2df6eMf/1rFSr5gDZLqOtGgXJTOvb1MO/R8pTmaNmbkpGp4X5ApbExzqyb9v4dPNna3qgPBJn0vwTw0RUyLPlFOjaMHBeoRuC77e2TNqVMXGxRRnlB25XJsCsFY/+bePsPQZXGIFB1gsMzDtB/M6WKcPuAsevZDrw==";
            string seed = "3Y9u5gc5dVKPhwiUzwmMV09ojummGzJ87A0llG9ey60=";
            string pgenCounter = "AWs=";
            string dsaKeyXml = $@"<KeyInfo xmlns=""http://www.w3.org/2000/09/xmldsig#""><KeyValue><DSAKeyValue><P>{p}</P><Q>{q}</Q><G>{g}</G><Y>{y}</Y><Seed>{seed}</Seed><PgenCounter>{pgenCounter}</PgenCounter></DSAKeyValue></KeyValue></KeyInfo>";

            var expected = new Dictionary<string, string>()
            {
                { "P", p },
                { "Q", q },
                { "G", g },
                { "Y", y },
                { "Seed", seed },
                { "PgenCounter", pgenCounter }
            };

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(dsaKeyXml);

            KeyInfo info = new KeyInfo();
            info.LoadXml(doc.DocumentElement);

            XmlElement el = info.GetXml();

            foreach (var kv in expected)
            {
                XmlNode node = el.SelectSingleNode($"//*[local-name()='DSAKeyValue']/*[local-name()='{kv.Key}']");
                Assert.NotNull(node);
                Assert.Equal(kv.Value, node.InnerText);
            }
        }
    }
}
