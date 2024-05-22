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


using PicoGK;


namespace Leap71
{
    using ShapeKernel;

    namespace Simulation
    {
        public class SimpleFlowDevice
        {
            // domains
            protected Voxels        m_voxFluidDomain;
            protected Voxels        m_voxSolidDomain;

            // patches
            protected Voxels        m_voxInletPatch;


            /// <summary>
            /// Class to provide the geometric input data for a simulation.
            /// This simple flow device generates a modulated cylinder with a gyroid section as the fluid domain.
            /// The solid domain is a modulated pipe around the cylinder.
            /// The inlet patch is a flat cylinder on the top end of the fluid domain.
            /// </summary>
            public SimpleFlowDevice()
            {
                // generate fluid domain: inner pipe
                float fPipeLength           = 150f;
                LocalFrame oPipeFrame       = new LocalFrame();
                BaseCylinder oInnerPipe     = new BaseCylinder(new LocalFrame(), fPipeLength);
                oInnerPipe.SetRadius(new SurfaceModulation(fGetInnerRadius));
                Voxels voxInnerPipe         = oInnerPipe.voxConstruct();


                // generate fluid domain: gyroid section
                float fGyroidUnitSize       = 10f;
                float fGyroidWallThickness  = 1f;
                IImplicit xGyroid           = new ImplicitGyroid(fGyroidUnitSize, ImplicitGyroid.fGetThicknessRatio(fGyroidWallThickness, fGyroidUnitSize));
                float fGyroidBoundRadius    = fGetInnerRadius(0, 0.5f) + 10f;
                float fGyroidBoundHeight    = 0.5f * fPipeLength;
                LocalFrame oGyroidFrame     = LocalFrame.oGetTranslatedFrame(oPipeFrame, 0.5f * (fPipeLength - fGyroidBoundHeight) * oPipeFrame.vecGetLocalZ());
                BaseCylinder oGyroidBound   = new BaseCylinder(oGyroidFrame, fGyroidBoundHeight, fGyroidBoundRadius);
                Voxels voxGyroid            = Sh.voxIntersectImplicit(oGyroidBound.voxConstruct(), xGyroid);
                m_voxFluidDomain            = Sh.voxSubtract(voxInnerPipe, voxGyroid);


                // generate oversized inlet patch bounding
                float fPatchThickness       = 4f;
                float fPatchRadius          = fGetInnerRadius(0f, 1f) + 5f;
                LocalFrame oPatchFrame      = LocalFrame.oGetTranslatedFrame(oPipeFrame, (fPipeLength - 0.5f * fPatchThickness) * oPipeFrame.vecGetLocalZ());
                BaseCylinder oInletPatch    = new BaseCylinder(oPatchFrame, fPatchThickness, fPatchRadius);
                m_voxInletPatch             = oInletPatch.voxConstruct();


                // generate solid part domain
                BaseCylinder oOuterPipe     = new BaseCylinder(new LocalFrame(), fPipeLength);
                oOuterPipe.SetRadius(new SurfaceModulation(fGetOuterRadius));
                m_voxSolidDomain            = oOuterPipe.voxConstruct();
                m_voxSolidDomain            = Sh.voxSubtract(m_voxSolidDomain, m_voxFluidDomain);


                // previews
                Voxels voxPreviewFluidDomain    = Sh.voxSubtract(m_voxFluidDomain, voxGetSegmentCut(oPipeFrame, fPipeLength, fGyroidBoundRadius, 0f, 90f));
                Voxels voxPreviewSolidDomain    = Sh.voxSubtract(m_voxSolidDomain, voxGetSegmentCut(oPipeFrame, fPipeLength, fGyroidBoundRadius, 0f, 180f));
                Voxels voxPreviewInletPatch     = m_voxInletPatch;
                Sh.PreviewVoxels(voxPreviewSolidDomain,  Cp.clrRock, 0.9f);
                Sh.PreviewVoxels(voxPreviewFluidDomain,  Cp.clrBlue, 0.6f);
                Sh.PreviewVoxels(voxPreviewInletPatch,   Cp.clrBillie, 0.5f);
            }

            /// <summary>
            /// Utility: Returns the outer radius of the fluid domain.
            /// </summary>
            protected float fGetInnerRadius(float fPhi, float fLR)
            {
                float fInletRadius  = 20f;
                float fMaxRadius    = 30f;
                float fOutletRadius = 15f;
                float fLR1          = Uf.fLimitValue((fLR - 0.0f) / 0.5f, 0f, 1f);
                float fLR2          = Uf.fLimitValue((fLR - 0.5f) / 0.5f, 0f, 1f);
                float fRadius       = Uf.fTransFixed(fInletRadius, fMaxRadius, fLR1);
                fRadius             = Uf.fTransFixed(fRadius, fOutletRadius, fLR2);
                return fRadius;
            }

            /// <summary>
            /// Utility: Returns the outer radius of the solid domain.
            /// </summary>
            protected float fGetOuterRadius(float fPhi, float fLR)
            {
                float fInnerRadius  = fGetInnerRadius(fPhi, fLR);
                float dFlangeRadius = 10f;
                float dWallRadius   = 2f;
                float fLR1          = Uf.fLimitValue((fLR - 0.0f) / 0.1f, 0f, 1f);
                float fLR2          = Uf.fLimitValue((fLR - 0.9f) / 0.1f, 0f, 1f);
                float dRadius       = Uf.fTransFixed(dFlangeRadius, dWallRadius, fLR1);
                dRadius             = Uf.fTransFixed(dRadius, dFlangeRadius, fLR2);
                float fRadius       = fInnerRadius + dRadius;
                return fRadius;
            }

            /// <summary>
            /// Utility: Creates a segment to cut domains and make a nicer preview.
            /// </summary>
            protected Voxels voxGetSegmentCut(LocalFrame oFrame, float fLength, float fOuterRadius, float fStartAngle, float fEndAngle)
            {
                LocalFrame oCutFrame        = LocalFrame.oGetTranslatedFrame(oFrame, -2f * oFrame.vecGetLocalZ());
                float fCutLength            = fLength + 4f;
                float fStartPhi             = fStartAngle / 180f * MathF.PI;
                float fEndPhi               = fEndAngle / 180f * MathF.PI;
                BasePipeSegment oSegment    = new ( oCutFrame,
                                                    fCutLength,
                                                    0.1f,
                                                    fOuterRadius,
                                                    new LineModulation(fStartPhi),
                                                    new LineModulation(fEndPhi),
                                                    BasePipeSegment.EMethod.START_END);
                return oSegment.voxConstruct();
            }

            /// <summary>
            /// Returns the fluid domain as a voxel field.
            /// </summary>
            public Voxels voxGetFluidDomain()
            {
                return m_voxFluidDomain;
            }

            /// <summary>
            /// Returns the solid domain as a voxel field.
            /// </summary>
            public Voxels voxGetSolidDomain()
            {
                return m_voxSolidDomain;
            }

            /// <summary>
            /// Returns an oversized boundary for the inlet patch.
            /// </summary>
            public Voxels voxGetInletPatch()
            {
                return m_voxInletPatch;
            }
        }
    }
}