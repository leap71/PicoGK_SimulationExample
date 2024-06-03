//
// SPDX-License-Identifier: CC0-1.0
//
// This example code file is released to the public under Creative Commons CC0.
// See https://creativecommons.org/publicdomain/zero/1.0/legalcode
//
// To the extent possible under law, LEAP 71 has waived all copyright and
// related or neighboring rights to this PicoGK example code file.
//
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//


using System.Numerics;
using PicoGK;


namespace Leap71
{
    using ShapeKernel;
    using static ShapeKernel.BaseShape;

    namespace Simulation
    {
        public class DisplacementCheck
        {
            public delegate float   ColorScaleFunc(Vector3 vecA, Vector3 vecB, Vector3 vecC);

            protected   Mesh            m_mshSolidDomain;
            protected   VectorField     m_oDisplacementField;
            protected   float           m_fScaleFactor;

            /// <summary>
            /// Class displaces and paints the geometry according to the vector field.
            /// </summary>
            public DisplacementCheck(   Voxels      voxSolidDomain,
                                        VectorField oDisplacementField,
                                        float       fMaxDisplayDisplacement)
            {
                m_mshSolidDomain        = new Mesh(voxSolidDomain);
                m_oDisplacementField    = oDisplacementField;


                // get estimation of max displacement magnitude
                float fMinDisplacement  = 0f;
                float fMaxDisplacement  = 0f;
                BBox3 oBBox             = Sh.oGetBoundingBox(voxSolidDomain);
                float fStep             = 2f;
                for (float fZ = oBBox.vecMin.Z; fZ <= oBBox.vecMax.Z; fZ += fStep)
                {
                    for (float fX = oBBox.vecMin.X; fX <= oBBox.vecMax.X; fX += fStep)
                    {
                        for (float fY = oBBox.vecMin.Y; fY <= oBBox.vecMax.Y; fY += fStep)
                        {
                            Vector3 vecPosition = new Vector3(fX, fY, oBBox.vecMax.Z - fZ);
                            bool bSuccess       = oDisplacementField.bGetValue(vecPosition, out Vector3 vecFieldValue);
                            if (bSuccess == true)
                            {
                                Vector3 vecDisplacement = vecFieldValue;
                                float fDisplacement     = vecDisplacement.Length();
                                if (fDisplacement > fMaxDisplacement)
                                {
                                    fMaxDisplacement = fDisplacement;
                                }
                            }
                        }
                    }
                }
                m_fScaleFactor = fMaxDisplayDisplacement / fMaxDisplacement;


                // apply mesh displacement, paint mesh and preview
                Library.oViewer().RemoveAllObjects();
                IColorScale xScale = new ColorScale3D(new RainboxSpectrum(), fMinDisplacement, fMaxDisplacement);
                PreviewCustomProperty(m_mshSolidDomain, xScale, fGetDisplacementMagnitude, vecGetDisplacement, 20);
            }

            /// <summary>
            /// Returns the displacement magnitude from the vector field for a given triangle.
            /// </summary>
            protected float fGetDisplacementMagnitude(Vector3 vecA, Vector3 vecB, Vector3 vecC)
            {
                float fMagnitude    = 0;

                Vector3 vecPosition = (vecA + vecB + vecC) / 3f;
                bool bSuccess       = m_oDisplacementField.bGetValue(vecPosition, out Vector3 vecFieldValue);
                if (bSuccess == true)
                {
                    Vector3 vecDisplacement = vecFieldValue;
                    fMagnitude              = vecDisplacement.Length();
                }
                return fMagnitude;
            }

            /// <summary>
            /// Returns the scaled displacement vector from the vector field applied to the given mesh vertex position.
            /// </summary>
            protected Vector3 vecGetDisplacement(Vector3 vecPt)
            {
                Vector3 vecDisplacement = new Vector3();

                Vector3 vecPosition     = vecPt;
                bool bSuccess           = m_oDisplacementField.bGetValue(vecPosition, out Vector3 vecFieldValue);
                if (bSuccess == true)
                {
                    vecDisplacement     = m_fScaleFactor * vecFieldValue;
                }
                return vecPt + vecDisplacement;
            }

            /// <summary>
            /// Divides the specified mesh into multiple sub-meshes from triangles that share a similar custom properties.
            /// </summary>
            public static void PreviewCustomProperty(   Mesh oMesh,
                                                        IColorScale xScale,
                                                        ColorScaleFunc oColorFunc,
                                                        TrafoFunc oDisplacementFunc,
                                                        uint nClasses = 10)
            {
                Mesh[] aSubMeshes   = new Mesh[nClasses];
                float fMinValue     = xScale.fGetMinValue();
                float fMaxValue     = xScale.fGetMaxValue();
                float dValue        = (fMaxValue - fMinValue) / (nClasses - 1f);

                for (int i = 0; i < nClasses; i++)
                {
                    aSubMeshes[i] = new Mesh();
                }

                uint nNumberOfTriangles = (uint)oMesh.nTriangleCount();
                for (int i = 0; i < nNumberOfTriangles; i++)
                {
                    oMesh.GetTriangle(i, out Vector3 vecA, out Vector3 vecB, out Vector3 vecC);

                    float fValue        = oColorFunc(vecA, vecB, vecC);
                    float fRatio        = (fValue - fMinValue) / (fMaxValue - fMinValue);
                    fRatio              = Uf.fLimitValue(fRatio, 0f, 1f);

                    uint nSubMeshIndex  = (uint)(fRatio * (nClasses - 1));
                    aSubMeshes[nSubMeshIndex].nAddTriangle(vecA, vecB, vecC);
                }

                for (int i = 1; i < nClasses; i++)
                {
                    ColorFloat clr      = xScale.clrGetColor(fMinValue + (i * dValue));
                    try
                    {
                        Mesh mshDisplaced = MeshUtility.mshApplyTransformation(aSubMeshes[i], oDisplacementFunc);
                        Sh.PreviewMesh(mshDisplaced, clr);
                    }
                    catch { }
                }
            }
        }
    }
}