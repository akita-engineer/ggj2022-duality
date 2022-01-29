using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using StarterAssets;

[RequireComponent(typeof(FirstPersonController))]
public class WarpBackIfFallen : MonoBehaviour
{
    public float recordDelay = 5.0f;
    public int recordingLimit = 5;
    public float yThreshold = -100.0f;

    private List<Vector3> mRecordings;
    private int mRecordIndex = 0;
    private float mCounter = 0;

    private FirstPersonController mController;

    private void TakeRecording()
    {
        if (mRecordIndex >= recordingLimit)
        {
            mRecordIndex = 0;
        }

        mRecordings[mRecordIndex] = transform.position;
    }

    private void WarpBack()
    {
        // note: we can use an even further back position if needed.
        // for example if notice due to island slopes they keep on sliding off even when warping back.
        transform.position = mRecordings[mRecordIndex];
    }

    private void Awake()
    {
        mRecordings = Enumerable.Repeat(transform.position, recordingLimit).ToList();
        mController = GetComponent<FirstPersonController>();
    }

    private void FixedUpdate()
    {
        mCounter += Time.fixedDeltaTime;
        if (mCounter >= recordDelay && mController.Grounded)
        {
            mCounter = 0;
            TakeRecording();
        }

        if (transform.position.y < yThreshold)
        {
            WarpBack();
        }
    }
}
